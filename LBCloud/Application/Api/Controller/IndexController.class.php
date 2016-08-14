<?php
namespace Api\Controller;
use Think\Controller;
use Api\Service\AliyunOSS;

class IndexController extends Controller {
    public function index()
    {
    	//$str = "RmeK6V8kOsuPV/LvJTjDvRMtyYvH9rZKtcMgcDVYMzI5vYJgqIGzknxTU82vf4ta+saxn5hljajWX/iW+SD7HVOx+NkQbJKuDoOWMqkwmVQefpCdIcwyFbxzONAgWrfQrNcvCFfumSm6UAEmzqwHv1D3JOLu+DSe6QzECXKACw50JYtXYBAef0JugdMionsu6eO7TbSTTKIysDVhmSMGT9QYFfK2+SObUM67CJdkpdjySPJRkcj0PnG23DOXuJt0nJT1V6i+GKziLb5nWuCVfCApmPal5BRRbPORxCif/Ag0i/cfvrKSR7DbtKKO0/1MPlz+L23Q8CwEDvrEvWp3C/GJsaiUrdjTyE8oUV17VKqR6gmyUbs+q9hVNau0T+ERs9ucv8WxVBV9hSTqfhAAjHhu+1S7ami0M5/1JrqmI7SsKKssYuZfOadoYV7hUFOz8IO73IbZs1eM266jcmizASNF8OLxljAyTo2j7YKf8PcDG2Y3n4XrHIGYhOwempSlj5i4fiNjMT/Fi5C2XPjWgjpbnO+/XH9b4P8dTgXHz4feTrk3JfqK5UFUL1/JEZQRzdUUusQL1u4QI8ek/64l3n6KSX8Swo+dji0Tv9f90OgBwfZB76PGbrsmCnvF6w7FhYE683fZPOPMb6/RYb4xGjDaHb9a/UWVnP753rzS8TAxgeb6x8s+FRjkFtUhz19wXVXRiOIbJ1SIPACwn0hZqIEpeDyT4Jb5Cuo/uAWbR4wXYTM9BXl6crbudO5jGcZfNoiOJWS4yBBvKuWnF3g7IJuihrJQtd7Hnq+Dh2JZFvzuLrp8wIBUOzeDKFMIXMdpYl+YmudzIYvkBmlBdDmBs/tFhBM9ERd5kM1Q+aGh/gbKaE26yQGFH/F563FP+H/C";
    	//echo decrypt($str);
		//exit;
		
    	//echo random_string(31);
		//exit;
		
		
    	//$screen_model = D("Screen");
		//$result = $screen_model->user_all_screen("1");
		//dump($result);
		//exit;
		
    	$AliyunOSS = new AliyunOSS();
		$bucket = C("aliyun_oss_bucket");
		//$object = "201608021433/wx_notify.php";
		$object = "57a3f9c7354bc.txt";
		$upload_id = "2713681012614B3F95D78F14B3AF44E2";
		
		$result = $AliyunOSS->generate_upload_part(2014568, 102400);
		//echo json_encode($result);
		dump($result);
		exit;
		
		//$result = $AliyunOSS->object_list($bucket);
		//dump($result);
		//exit;
		
		//$AliyunOSS->complete_upload("57a31bb736fda.txt", "F77135AFC16A4F538211515502EC1CFD", array());
		
		//$result = $AliyunOSS->upload_part_list();
		//dump($result);
		
		//$result = $AliyunOSS->part_list($object, $upload_id);
		//dump($result);
		//exit;
		
		//$result = $AliyunOSS->get_upload_id(".txt");
		//dump($result);

		//$result = $AliyunOSS->upload_part_sign($object, $upload_id, 3, false, 3600);
		//echo $result;
		
		//$buckets = $AliyunOSS->bucket_list();
		//dump($buckets);
		
		//$result = $AliyunOSS->download_uri($bucket, $object);
		//dump($result);
		
		//$result = $AliyunOSS->object_meta($bucket, $object);
		//dump($result);
		
		
		//$result = $AliyunOSS->abort_upload("57a3fcd14549a.txt", "E1B36EBA4C9946C3BFAAFDC836950172");
		//dump($result);
		
		/*$parts = [
			['PartNumber' => 1,'ETag' => "1968AF318A72C56BDB3BE8B26F09EA3B"],
			['PartNumber' => 2,'ETag' => "97908FC7F9A744CA8C5DA2643000A249"],
			['PartNumber' => 3,'ETag' => "A7B7C3CD26EF3C18B43AD6CA6B041122"],
		];
		$result = $AliyunOSS->complete_upload($object, $upload_id, $parts);
		dump($result);
		
		$result = $AliyunOSS->download_uri($bucket, $object);
		dump($result);*/
    }
}