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
		$object = "201608021433/wx_notify.php";
		
		//$result = $AliyunOSS->generate_upload_part(2014568, 102400);
		//dump($result);
		
		//$result = $AliyunOSS->object_list($bucket, "201608021433/");
		//dump($result);
		
		//$AliyunOSS->complete_upload("57a31bb736fda.txt", "F77135AFC16A4F538211515502EC1CFD", array());
		
		//$result = $AliyunOSS->upload_part_list();
		//dump($result);
		
		//$result = $AliyunOSS->part_list("57a31bcc1846c.txt", "A61D6A9B644B41B7A20A3CAD2C49BAC7");
		//dump($result);
		
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