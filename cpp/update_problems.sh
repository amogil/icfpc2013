curl -d '' 'http://icfpc2013.cloudapp.net/myproblems?auth=0071PimxQKpGJdtDE76gsjAoaOagBVX3tdGOfCQHvpsH1H' \
	| sed 's/},{/},\n{/g'\
	| grep -v true\
	| grep -v false\
	| grep -v fold \
	| sort -t ':' -k 3 -n > real_problems.txt
