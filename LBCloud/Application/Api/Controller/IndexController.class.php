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
		/*$bucket = C("aliyun_oss_bucket");
		$object = "20160903/57ca825f7502b.mp4";
		$upload_id = "BE7AD774AA4D4CC6BBB9CB6CBD3C6C36";*/
		$media_bucket = C("oss_media_bucket");
		$program_bucket = C("oss_program_bucket");
		$picture_bucket = C("oss_picture_bucket");
		
		//$AliyunOSS->demo($program_bucket, $object);
		
		/*$filesize = 3145728;
		$result = $AliyunOSS->part_size($filesize);
		dump($result);*/
		//exit;
		//分片
		//$result = $AliyunOSS->generate_upload_part($filesize, $result);
		//echo json_encode($result);
		//dump($result);
		//exit;

		//文件列表
		/*$url = $AliyunOSS->download_uri($picture_bucket, "D07E355210CB/20161022/1477127895.jpg");
		dump($url);
		exit;*/
		$result = $AliyunOSS->object_list($program_bucket, "20161207/");
		foreach($result['objects'] as $val){
			$url = $AliyunOSS->download_uri($program_bucket, $val['key']);
			dump($url);
			/*$result = $AliyunOSS->delete_object($val['key'], $picture_bucket);
			dump($result);*/
		}
		/*foreach($objs as $val){
			$result = $AliyunOSS->delete_object($val, $media_bucket);
			dump($result);
		}*/
		//exit;
		
		/*$result = $AliyunOSS->object_list($program_bucket);
		dump($result);
		foreach($result['prefixs'] as $val){
			$result = $AliyunOSS->object_list($program_bucket, $val['prefix']);
			foreach($result['objects'] as $val){
				$objs[] = $val['key'];
			}
		}
		dump($objs);
		foreach($objs as $val){
			$result = $AliyunOSS->delete_object($val, $program_bucket);
			dump($result);
		}*/
		//exit;
		
		
		/*
		$result = $AliyunOSS->object_list($media_bucket,  "20160903/");
		dump($result);
		
		
		$result = $AliyunOSS->object_list($program_bucket, "20160831/");
		dump($result);
		*/
		/*$result = $AliyunOSS->object_list($media_bucket);
		dump($result);
		exit;*/
		
		//完成上传，合并文件
		//$AliyunOSS->complete_upload("57a31bb736fda.txt", "F77135AFC16A4F538211515502EC1CFD", array());
		
		//uploadid列表
		/*$result = $AliyunOSS->upload_part_list($media_bucket);
		dump($result);
		foreach($result as $val){
			$AliyunOSS->abort_upload($val['key'], $val['uploadId'], $media_bucket);
		}
		$result = $AliyunOSS->upload_part_list($program_bucket);
		dump($result);
		foreach($result as $val){
			$AliyunOSS->abort_upload($val['key'], $val['uploadId'], $program_bucket);
		}
		exit;*/
		
		//上传文件签名地址
		//$result = $AliyunOSS->upload_sign_uri($object, $program_bucket);
		//dump($result);
		
		//上传成功分片文件列表
		//$result = $AliyunOSS->part_list($object, $upload_id, $media_bucket);
		//dump($result);
		//exit;
		
		//存储对象是否存在
		/*$result = $AliyunOSS->object_exists($object, $media_bucket);
		dump($result);
		exit;*/
		
		//$result = $AliyunOSS->get_upload_id(".txt");
		//dump($result);
		
		//上传分片文件签名地址
		//$result = $AliyunOSS->upload_part_sign($object, $upload_id, 1, false, 3600);
		//echo $result;
		
		//bucket列表
		/*$buckets = $AliyunOSS->bucket_list();
		dump($buckets);
		exit;*/
		
		//下载地址
		//$result = $AliyunOSS->download_uri($program_bucket, $object, 3600);
		//echo $result;
		
		//文件信息
		//$result = $AliyunOSS->object_meta($program_bucket, $object);
		//dump($result);
		
		//中止上传
		//$result = $AliyunOSS->abort_upload("57a3fcd14549a.txt", "E1B36EBA4C9946C3BFAAFDC836950172");
		//dump($result);
		
		//上传成功
		/*$parts = array(
			array('PartNumber' => 1,'ETag' => "42A85F0C53035D7FA05E73D1A35CD3D3"),
			array('PartNumber' => 2,'ETag' => "1977A6725CD0DF48409881C5EBCB9D5C"),
			array('PartNumber' => 3,'ETag' => "6F1D0D4D3B16F16C57E4562C432B6866"),
			array('PartNumber' => 4,'ETag' => "38EAB8682D4A420DDEC747927508F499"),
			array('PartNumber' => 5,'ETag' => "34D38E57F64423BEEB1D514A52DA466E"),
			array('PartNumber' => 6,'ETag' => "E8CA766727669C7E0505E650EF72986C")
		);
		$result = $AliyunOSS->complete_upload($object, $upload_id, $parts, $media_bucket);
		dump($result);*/
		
		//下载地址
		/*$result = $AliyunOSS->download_uri($picture_bucket, "D07E355210CB/20161022/1477126651.jpg");
		dump($result);*/
    }
}