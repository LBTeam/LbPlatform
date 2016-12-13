<?php
namespace Api\Controller;
use Think\Controller;
use Api\Service\AliyunOSS;

class DemoController extends Controller
{
	public function demo_tes(){
		$filepath = 'C:\\Users\\Hardborn\\AppData\\Roaming\\LBManager\\Media\\Test1"11.playprog';
		$filename = end(explode('/', str_replace('\\', '/', $filepath)));
		dump($filename);
		$demo = mysql_real_escape_string($filename);
		dump($demo);
		$demo = mysql_escape_string($filename);
		dump($demo);
		$demo = addslashes($filename);
		dump($demo);
	}
	
	
	public function demo(){
		exit;
		$media['name'] = "1.2.3.png";
		$v['MediaMD5'] = "md5md5md5md5md5";
		dump($media['name']);
		$media_name_array = explode('.', stripslashes($media['name']));
		$suffix = end($media_name_array);
		$temp_name = array_pop($media_name_array);
		$media_name = implode('.', $media_name_array) . "_" . $v['MediaMD5'] . "." . $suffix;
		dump($media_name);
		exit;
		$response = array('err_code'=>'010101', 'msg'=>"User does not exist");
		$this->ajaxReturn($response);
	}
	
	public function index(){
		//exit;
		/*$AliyunOSS = new AliyunOSS();
		$result = $AliyunOSS->bucket_list();
		dump($result);
		exit;*/
		/*$result = $AliyunOSS->create_bucket(C("oss_picture_bucket"));
		exit;*/
		/*$file = "./test.png";
		$filesize = filesize($file);
		dump($filesize);*/
		
		//分片大小
		/*$partsize = $AliyunOSS->part_size($filesize);
		dump($partsize);*/
		
		//分片情况
		/*$parts = $AliyunOSS->generate_upload_part($filesize, $partsize);
		dump($parts);*/
		$AliyunOSS = new AliyunOSS();
		$media_bucket = C("oss_media_bucket");
		//获取upload_id
		//$result = $AliyunOSS->get_upload_id("png", $media_bucket);
		//dump($result);
		$object = "20161025/580f1909d8f61.MOV";
		$upload_id = "4C6C734773D4429183711C8749BB8AF9";
		
		//生成分片文件
		/*$handle = fopen($file, 'r+');
		$i = 1;
		while (!feof($handle)) {
			$temp = fread($handle, $partsize);
			file_put_contents("temp{$i}", $temp);
			$i++;
		}
		fclose($handle);*/
		
		//生成分片文件curl上传命令
		/*foreach($parts as $key=>$val){
			$number = $key+1;
		  	//分片文件put地址
			$part_uri = $AliyunOSS->upload_part_sign($object, $upload_id, $number, $media_bucket, false, 1200);
			$curl_string = "curl -T /d/www/LbPlatform/LBCloud/temp{$number} '{$part_uri}' -H 'Content-Type: application/octet-stream'";
			echo $curl_string;
			echo "<br>";
		}*/
		
		//成功上传的分片
		/*$part_list = $AliyunOSS->part_list($object, $upload_id, $media_bucket);
		dump($part_list);*/
		
		//合并分片文件
		/*$parts = array(
			array('PartNumber' => 1,'ETag' => "8D26A0D623E4BDBC2FE6C6FF6F2D25F5"),
			array('PartNumber' => 2,'ETag' => "7595E95A5CAFDAD1AE1EBB4A9BFD5081")
		);
		$result = $AliyunOSS->complete_upload($object, $upload_id, $parts, $media_bucket);
		dump($result);*/
		
		//下载地址
		$download_uri = $AliyunOSS->download_uri($media_bucket, $object);
		dump($download_uri);
		
		/*$result = $AliyunOSS->object_list($media_bucket, "20160919/");
		dump($result);*/
		
		/*$result = $AliyunOSS->upload_part_list($media_bucket);
		dump($result);
		foreach($result as $val){
			$parts = $AliyunOSS->part_list($val['key'], $val['uploadId'], $media_bucket);
			$ps = array();
			foreach($parts as $vo){
				$ps[] = array(
					'PartNumber' => $vo['partNumber'],
					'ETag'	=> trim($vo['eTag'], '"')
				);
			}
			$temp = $AliyunOSS->complete_upload($val['key'], $val['uploadId'], $ps, $media_bucket);
			dump($temp);
		}*/
		
		/*$result = $AliyunOSS->part_list($object, $upload_id, $media_bucket);
		dump($result);
		
		$parts = array(
			array('PartNumber' => 1,'ETag' => "A2A0F4A7D102803398F5C192FE6ACB6E"),
			array('PartNumber' => 2,'ETag' => "C4742AC1F6ACFAF178CAED1D3063769E"),
			array('PartNumber' => 3,'ETag' => "E3A4303BDC906219189538AA062FA7CC"),
			array('PartNumber' => 4,'ETag' => "B96EE6C5F0EE6309A7F508E3A63A44EB"),
			array('PartNumber' => 5,'ETag' => "5F81EDB07CFFB81372C5594461CC5049"),
			array('PartNumber' => 6,'ETag' => "A098533B0E8E023B7D7DB50E5EB7054D"),
			array('PartNumber' => 7,'ETag' => "52E4BA13000D1E7BA5E6BC0899BFC3DC")
		);
		$result = $AliyunOSS->complete_upload($object, $upload_id, $parts, $media_bucket);
		dump($result);*/
	}
}
