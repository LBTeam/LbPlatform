<?php
namespace app\api\controller;
use app\api\service\AliyunOSS;

class Index
{
    public function index()
    {
    	$AliyunOSS = new AliyunOSS();
		/*$buckets = $AliyunOSS->bucket_list();
		dump($buckets);
		$bucket = config("aliyun_oss_bucket");
		$object = "wx_notify.php";
		$result = $AliyunOSS->download_uri($bucket, $object);
		dump($result);
		$result = $AliyunOSS->object_meta($bucket, $object);
		dump($result);
		echo strtotime($result['last']);*/
		$AliyunOSS->demo1();
    }
}
