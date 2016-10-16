<?php
namespace Index\Controller;
use Think\Controller;
use Think\Cache;

class DemoController extends Controller
{
	public function socket(){
		import("@.Service.WebsocketClient", '', ".php");
		$client = new \WebSocketClient();
		$client->connect("123.56.240.172", 9501, '/');
		//$in   = '{"Act": "notice","Id": "R3dAUyFk","Key": "nKpYrjsx5UdPuMS4","Mac": "4C-CC-6A-05-70-7B", "Content": "hello client"}';
		$in   = '{"Act": "shutdown","Id": "R3dAUyFk","Key": "nKpYrjsx5UdPuMS4","Mac": "4C-CC-6A-05-70-7B"}';
		$rs = $client->sendData($in);
	
		if( $rs !== true ){
			echo "sendData error...\n";
		}else{
			echo "ok\n";
		}
		unset($client);
	}
	
	
	public function redis(){
		$redis_serv = Cache::getInstance('Redis', array('host'=>"10.171.126.247"));
		$cache_key = md5("test_key");
		$res = $redis_serv->get($cache_key);
		dump($res);
		$res = $redis_serv->set($cache_key, "test");
		dump($res);
		$res = $redis_serv->get($cache_key);
		dump($res);
		unset($redis_serv);
	}
	
	public function redis2(){
		$redis_serv = Cache::getInstance('Redis', array('host'=>"10.171.126.247"));
		$cache_key = "86d7c78a4b524eb06eb2186ddeb4188a";
		$res = $redis_serv->get($cache_key);
		dump($res);
		unset($redis_serv);
	}
	
	public function index(){
		echo session("15934854815_code");
		exit;
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
