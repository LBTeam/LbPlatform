<?php
namespace Api\Controller;
use Think\Controller;
use Api\Service\AliyunOSS;

class IndexController extends Controller {
    public function index()
    {
    	/*echo sp_password("123456");
		exit;*/
    	
    	/*$start = "2016-08-02";
		$end = "2016-08-02";
		if($start < $end){
			echo "<";
		}else{
			echo ">=";
		}
		exit;*/
		
    	/*$array = array();
		$array['FileName'] = 'aabbcc.jpg';
		$array['FileMD5'] = 'b3206b4529ba377b0fa9f4a3bd9261f2';
		$array['Parts'] = array(
			array('partNumber'=>1, 'MD5'=>'96b71273c03d44bb9c81c6c37c640cf0'),
			array('partNumber'=>2, 'MD5'=>'96b71273c03d44bb9c81c6c37c640cf0')
		);
		echo json_encode($array);
		exit;*/
		
		
    	//$str = "RmeK6V8kOsuPV/LvJTjDvRMtyYvH9rZKtcMgcDVYMzI5vYJgqIGzknxTU82vf4ta+saxn5hljajWX/iW+SD7HVOx+NkQbJKuDoOWMqkwmVQefpCdIcwyFbxzONAgWrfQrNcvCFfumSm6UAEmzqwHv1D3JOLu+DSe6QzECXKACw50JYtXYBAef0JugdMionsu6eO7TbSTTKIysDVhmSMGT9QYFfK2+SObUM67CJdkpdjySPJRkcj0PnG23DOXuJt0nJT1V6i+GKziLb5nWuCVfCApmPal5BRRbPORxCif/Ag0i/cfvrKSR7DbtKKO0/1MPlz+L23Q8CwEDvrEvWp3C/GJsaiUrdjTyE8oUV17VKqR6gmyUbs+q9hVNau0T+ERs9ucv8WxVBV9hSTqfhAAjHhu+1S7ami0M5/1JrqmI7SsKKssYuZfOadoYV7hUFOz8IO73IbZs1eM266jcmizASNF8OLxljAyTo2j7YKf8PcDG2Y3n4XrHIGYhOwempSlj5i4fiNjMT/Fi5C2XPjWgjpbnO+/XH9b4P8dTgXHz4feTrk3JfqK5UFUL1/JEZQRzdUUusQL1u4QI8ek/64l3n6KSX8Swo+dji0Tv9f90OgBwfZB76PGbrsmCnvF6w7FhYE683fZPOPMb6/RYb4xGjDaHb9a/UWVnP753rzS8TAxgeb6x8s+FRjkFtUhz19wXVXRiOIbJ1SIPACwn0hZqIEpeDyT4Jb5Cuo/uAWbR4wXYTM9BXl6crbudO5jGcZfNoiOJWS4yBBvKuWnF3g7IJuihrJQtd7Hnq+Dh2JZFvzuLrp8wIBUOzeDKFMIXMdpYl+YmudzIYvkBmlBdDmBs/tFhBM9ERd5kM1Q+aGh/gbKaE26yQGFH/F563FP+H/C";
    	//echo decrypt($str);
		//exit;
		
    	//echo random_string(10);
		//exit;
		
		
    	//$screen_model = D("Screen");
		//$result = $screen_model->user_all_screen("1");
		//dump($result);
		//exit;
		
    	$AliyunOSS = new AliyunOSS();
		$bucket = C("aliyun_oss_bucket");
		$object = "20160816/57b2c0ee04398.playprog";
		$upload_id = "96B71273C03D44BB9C81C6C37C640CF0";
		$media_bucket = C("oss_media_bucket");
		$program_bucket = C("oss_program_bucket");
		
		$AliyunOSS->demo($program_bucket, $object);
		
		//分片
		//$result = $AliyunOSS->generate_upload_part(2014568, 102400);
		//echo json_encode($result);
		//dump($result);
		//exit;
		
		//文件列表
		//$result = $AliyunOSS->object_list($program_bucket, '20160816/');
		//dump($result);
		//exit;
		
		//完成上传，合并文件
		//$AliyunOSS->complete_upload("57a31bb736fda.txt", "F77135AFC16A4F538211515502EC1CFD", array());
		
		//uploadid列表
		//$result = $AliyunOSS->upload_part_list($media_bucket);
		//dump($result);
		//foreach($result as $val){
		//	$AliyunOSS->abort_upload($val['key'], $val['uploadId'], $media_bucket);
		//}
		//$result = $AliyunOSS->upload_part_list($program_bucket);
		//dump($result);
		//foreach($result as $val){
		//	$AliyunOSS->abort_upload($val['key'], $val['uploadId'], $program_bucket);
		//}
		
		//上传文件签名地址
		//$result = $AliyunOSS->upload_sign_uri($object, $program_bucket);
		//dump($result);
		
		//上传成功分片文件列表
		//$result = $AliyunOSS->part_list($object, $upload_id);
		//dump($result);
		//exit;
		
		
		//$result = $AliyunOSS->get_upload_id(".txt");
		//dump($result);
		
		//上传分片文件签名地址
		//$result = $AliyunOSS->upload_part_sign($object, $upload_id, 1, false, 3600);
		//echo $result;
		
		//bucket列表
		//$buckets = $AliyunOSS->bucket_list();
		//dump($buckets);
		
		//下载地址
		//$result = $AliyunOSS->download_uri($program_bucket, $object);
		//dump($result);
		
		//文件信息
		//$result = $AliyunOSS->object_meta($bucket, $object);
		//dump($result);
		
		//中止上传
		//$result = $AliyunOSS->abort_upload("57a3fcd14549a.txt", "E1B36EBA4C9946C3BFAAFDC836950172");
		//dump($result);
		
		//上传成功
		/*$parts = array(
			array('PartNumber' => 1,'ETag' => "1968AF318A72C56BDB3BE8B26F09EA3B"),
			array('PartNumber' => 2,'ETag' => "97908FC7F9A744CA8C5DA2643000A249"),
			array('PartNumber' => 3,'ETag' => "A7B7C3CD26EF3C18B43AD6CA6B041122"),
		);
		$result = $AliyunOSS->complete_upload($object, $upload_id, $parts);
		dump($result);
		
		//下载地址
		$result = $AliyunOSS->download_uri($bucket, $object);
		dump($result);*/
    }
}