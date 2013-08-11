
id=`tail -1 task_description.txt | sed 's/.*id":"//' | sed 's/".*//'`

echo $id
echo -n "$id: " >> log.txt

arguments=`cat values.txt | sed 's/\n//'`
#echo $arguments

task_description=`tail -1 task_description.txt | sed 's/\n//'`
echo -n $task_description > input.txt
echo >> input.txt

got_response=
while [ "$got_response" == "" ]
do
	curl -s -i -d '{"id":"'${id}'","arguments":'${arguments}'}' "http://icfpc2013.cloudapp.net/eval?auth=0071PimxQKpGJdtDE76gsjAoaOagBVX3tdGOfCQHvpsH1H" \
		| tee answers.txt
	code=`grep HTTP answers.txt | tail -1 | awk '{print $2}'`
	if [ "$code" == "200" ]
	then
		got_response=1
	else
		sleep 1
	fi
done

tail -1 answers.txt >> input.txt
echo >> input.txt

solved=

while [ "$solved" == "" ]
do
	./calc | tee output.txt

	lambda=`tail -1 output.txt | sed 's/\n//'`

	echo $lambda
	if [ "$lambda" == "Fail" ]
	then
		solved=0
		echo "Fail" >> log.txt
	else

		got_response=
		while [ "$got_response" == "" ]
		do
			curl -s -i -d '{"id":"'${id}'","program":"'"${lambda}"'"}' "http://icfpc2013.cloudapp.net/guess?auth=0071PimxQKpGJdtDE76gsjAoaOagBVX3tdGOfCQHvpsH1H" \
				| tee response.txt
			code=`grep HTTP response.txt | tail -1 | awk '{print $2}'`
			if [ "$code" == "200" ]
			then
				got_response=1
			elif [ "$code" == "410" ]
			then
				echo "Time limit exceeded" >> log.txt
				got_response=0
				solved=0
			else
				sleep 1
			fi
		done

		if [ "$got_response" == "1" ]
		then
			status=`tail -1 response.txt | awk -F '"' '{print $4}'`
			if [ "$status" == "mismatch" ]
			then
				tail -1 response.txt >> input.txt
				echo >> input.txt
			else
				solved=1
				echo "Solved" >> log.txt
			fi
		fi
	fi
done

