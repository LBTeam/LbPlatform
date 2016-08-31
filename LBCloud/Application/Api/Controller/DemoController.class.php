<?php
namespace Api\Controller;
use Think\Controller;
use Api\Service\AliyunOSS;

class DemoController extends Controller
{
	public function index(){
		$AliyunOSS = new AliyunOSS();
		$file = "./test.png";
		$filesize = filesize($file);
		dump($filesize);
		$partsize = $AliyunOSS->part_size($filesize);
		dump($partsize);
		$parts = $AliyunOSS->generate_upload_part($filesize, $partsize);
		dump($parts);
		$media_bucket = C("oss_media_bucket");
		//$result = $AliyunOSS->get_upload_id("png", $media_bucket);
		//dump($result);
		$object = "20160831/57c6ee3e2e2e8.png";
		$upload_id = "9101C75A4C3045E5BF5ADC863BA96890";
		/*$handle = fopen($file, 'r+');
		$i = 1;
		while (!feof($handle)) {
			$temp = fread($handle, $partsize);
			file_put_contents("temp{$i}", $temp);
			$i++;
		}
		fclose($handle);*/
		
		/*foreach($parts as $key=>$val){
			$number = $key+1;
			$part_uri = $AliyunOSS->upload_part_sign($object, $upload_id, $number, $media_bucket, false, 1200);
			$curl_string = "curl -T /d/www/LbPlatform/LBCloud/temp{$number} '{$part_uri}' -H 'Content-Type: application/octet-stream'";
			echo $curl_string;
			echo "<br>";
		}*/
		
		/*$part_list = $AliyunOSS->part_list($object, $upload_id, $media_bucket);
		dump($part_list);*/
		
		/*$parts = array(
			array('PartNumber' => 1,'ETag' => "8D26A0D623E4BDBC2FE6C6FF6F2D25F5"),
			array('PartNumber' => 2,'ETag' => "7595E95A5CAFDAD1AE1EBB4A9BFD5081")
		);
		$result = $AliyunOSS->complete_upload($object, $upload_id, $parts, $media_bucket);
		dump($result);*/
		
		$download_uri = $AliyunOSS->download_uri($media_bucket, $object);
		dump($download_uri);
		
	}
}
