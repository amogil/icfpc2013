id=$1

#echo "->$id<-"
#exit 0

grep $id real_problems.txt > task_description.txt

./solve2.sh
