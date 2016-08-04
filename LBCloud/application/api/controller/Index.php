<?php
namespace app\api\controller;
use app\api\service\AliyunOSS;

class Index
{
    public function index()
    {
    	$AliyunOSS = new AliyunOSS();
		$result = $AliyunOSS->get_upload_id(".txt");
		dump($result);
		$result = $AliyunOSS->upload_part_sign($result['Key'], $result['UploadId']);
		dump($result);
		/*$buckets = $AliyunOSS->bucket_list();
		dump($buckets);
		$bucket = config("aliyun_oss_bucket");
		$object = "wx_notify.php";
		$result = $AliyunOSS->download_uri($bucket, $object);
		dump($result);
		$result = $AliyunOSS->object_meta($bucket, $object);
		dump($result);
		echo strtotime($result['last']);*/
		//$AliyunOSS->demo1();
    }
}
