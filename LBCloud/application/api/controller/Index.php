<?php
namespace app\api\controller;
use app\api\service\AliyunOSS;

class Index
{
    public function index()
    {
    	$AliyunOSS = new AliyunOSS();
		$buckets = $AliyunOSS->bucket_list();
		dump($buckets);
		$bucket = "lb-player-test";
		$object = "wx_notify.php";
		$result = $AliyunOSS->download_uri($bucket, $object);
		dump($result);
    }
}
