cat problems.txt | while read line; do for word in $line; do echo $word; done; done | sort | uniq -c | sort -r -n > freq.txt 