#!/bin/bash
all_params=$@
start_test_no=0
end_test_no=0
specific_make_target=`echo " "${all_params}" " | sed -n 's/\(.*\)-mkt \([a-zA-Z][a-zA-Z_-]*\) \(.*\)/\2/p'`
test_to_run_interval=`echo ${all_params} | sed -n 's/\(.*\)-t \([0-9][0-9]*\(-[0-9]\+\)\?\)\(.*\)/\2/p'`
mono_params=`echo ${all_params} | sed 's/\(.*\)-t \([0-9][0-9]*\(-[0-9]\+\)\?\)\(.*\)/\1\4/'`
test_silent=`echo " "${all_params}" " | sed -n 's/\(.*\) \(-s\)\+ \(.*\)/\2/p'`
mono_params=`echo ${mono_params}" " | sed -e 's/-s//g' -e 's/-mkt [a-zA-Z][a-zA-Z_-]\+ //' -e 's/-mkt//'`
#echo $all_params
#echo "${test_to_run_interval}"
#echo "${mono_params}"
#echo "$test_silent"
#echo ${#test_silent}
start_test_no=`echo ${test_to_run_interval} | sed 's/\([0-9]*\).*/\1/'`
end_test_no=`echo ${test_to_run_interval} | sed 's/.*\([-]\)\([0-9]*\).*/\2/'`
#end_test_no=`echo ${end_test_no} | sed 's/\([0-9]*\).*/\1/'`
#echo "${start_test_no}"
#echo "${end_test_no}"
#echo ${specific_make_target}
if [ -z ${specific_make_target} ]; 
then
	specific_make_target=check
fi

if [ -z ${start_test_no} ]; 
then
	start_test_no=0
fi

if [ -z ${end_test_no} ]; 
then
	end_test_no=0
fi

if [ ${end_test_no} -lt ${start_test_no} ]; 
then
	tmp=${start_test_no}
	start_test_no=${end_test_no}
	end_test_no=${tmp}
fi

len=${#test_silent}
#echo ${len}
#echo ${specific_make_target}
if [ ${len} -gt 0 ]; 
then
	echo "invoke silent run"	
	export SILENT_TESTS=1	
else
	export SILENT_TESTS=
fi

x=${start_test_no}
while [ ${x} -le ${end_test_no} ]
do
	make ${specific_make_target} test_to_run=${x} MONO_CMD_PARAMS="${mono_params}"
	x=$(( $x+1 ))
done
