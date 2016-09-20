<?php
namespace Index\Controller;
use Think\Controller;

class DemoController extends Controller
{
	public function index(){
		echo random_string(32);
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
}
