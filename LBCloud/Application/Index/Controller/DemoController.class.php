<?php
namespace Index\Controller;
use Think\Controller;

class DemoController extends Controller
{
	public function index(){
		echo random_string(16);
	}
	
	public function demo(){
		$code = "1111111111";
		$code_list = array("1111111111","22222222223");
		$i = 0;
		while(in_array($code, $code_list)){
			if($i == 0){
				$code = "2222222222";
			}else{
				$code = random_string(12);
			}
			$i++;
		}
		dump($code);
	}
	
	function time2boolen(){
		$b1 = strtotime("08:00");
		$e1 = strtotime("09:00");
		$b2 = strtotime("08:59");
		$e2 = strtotime("10:00");
		$res = $this->is_time_cross($b1, $e1, $b2, $e2);
		dump($res);
	}
	
	function is_time_cross($beginTime1 = '', $endTime1 = '', $beginTime2 = '', $endTime2 = '')
	{
		$status = $beginTime2 - $beginTime1;
		if ($status > 0)
		{
			$status2 = $beginTime2 - $endTime1;
			if ($status2 >= 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		else
		{
			$status2 = $endTime2 - $beginTime1;
			if ($status2 > 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
