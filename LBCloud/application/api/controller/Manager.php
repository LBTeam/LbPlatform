<?php
/**
 * 编辑端接口控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace app\api\controller;
use app\api\service\AliyunOSS;

class Manager
{
	public function index(){
		$request = file_get_contents('php://input');
		$token = input("request.token");
		
		$proObj = json_decode($request);
		$proId = $proObj->Id;
		$param = $proObj->Para;
		
		$respones = array();
		
		if(empty($proId)){
			$respones = array("err_code"=>"100", "msg"=>"");
		}
		return 
	}
}
