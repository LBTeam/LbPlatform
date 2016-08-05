<?php
namespace app\api\controller;
use app\api\service\AliyunOSS;

class Index
{
    public function index()
    {
    	$AliyunOSS = new AliyunOSS();
		$bucket = config("aliyun_oss_bucket");
		$object = "201608021433/wx_notify.php";
		
		//$result = $AliyunOSS->generate_upload_part(2014568, 102400);
		//dump($result);
		
		//$result = $AliyunOSS->object_list($bucket, "201608021433/");
		//dump($result);
		
		//$AliyunOSS->complete_upload("57a31bb736fda.txt", "F77135AFC16A4F538211515502EC1CFD", array());
		
		//$result = $AliyunOSS->upload_part_list();
		//dump($result);
		
		$result = $AliyunOSS->part_list("57a31bcc1846c.txt", "A61D6A9B644B41B7A20A3CAD2C49BAC7");
		dump($result);
		
		//$result = $AliyunOSS->get_upload_id(".txt");
		//dump($result);

		//$result = $AliyunOSS->upload_part_sign($result['Key'], $result['UploadId']);
		//dump($result);
		
		//$buckets = $AliyunOSS->bucket_list();
		//dump($buckets);
		
		//$result = $AliyunOSS->download_uri($bucket, $object);
		//dump($result);
		
		//$result = $AliyunOSS->object_meta($bucket, $object);
		//dump($result);
    }
}
