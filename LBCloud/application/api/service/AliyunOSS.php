<?php
/**
 * 阿里云oss存储类
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace app\api\service;
use OSS\OssClient;
use OSS\Core\OssException;

class AliyunOSS
{
	private $client;
	public function __construct(){
		require_once(APP_PATH."/api/org/oss/autoload.php");
		$accessKeyId = config("aliyun_oss_id");
		$accessKeySecret = config("aliyun_oss_secret");
		$endpoint = config("aliyun_oss_endpoint");
		$bucket = config("aliyun_oss_bucket");
		$this->client = new OssClient($accessKeyId, $accessKeySecret, $endpoint);
	}
	
	/**
	 * 存储空间列表
	 * @return obj
	 */
	public function bucket_list(){
		try {
			$buckets = $this->client->listBuckets();
			return $buckets;
		} catch (OssException $e) {
		    print $e->getMessage();
		}
	}
	
	/**
	 * 获取私有存储空间对象下载地址
	 * @param $bucket 存储空间
	 * @param $object 存储对象
	 * @param $timeout 下载地址有效时间（秒）
	 * @return string
	 */
	public function download_uri($bucket, $object, $timeout=600){
		try {
			$result = $this->client->signUrl($bucket, $object, $timeout);
			return $result;
		} catch (OssException $e) {
		    print $e->getMessage();
		}
	}
	
	/**
	 * 获取存储对象meta信息
	 * @param $bucket 存储空间
	 * @param $object 存储对象
	 * @return array
	 */
	public function object_meta($bucket, $object){
		try {
			$respones = $this->client->getObjectMeta($bucket, $object);
			$result = array();
			$result['name'] = $object;
			$result['md5'] = str_replace('"', '', $respones['etag']);
			$result['size'] = $respones['content-length'];
			$result['ranges'] = $respones['accept-ranges'];
			$result['last'] = $respones['last-modified'];
			return $result;
		} catch (OssException $e) {
		    print $e->getMessage();
		}
	}
	
	public function demo1(){
		$res = $this->client->generateMultiuploadParts(1024001, 102400);
		echo json_encode($res);
	}
	
	public function demo(){
		
		try {
		    $ossClient = new OssClient($accessKeyId, $accessKeySecret, $endpoint);
			/*$result = $ossClient->deleteObject($bucket, "201608021433");
			dump($result);
			exit;*/
			//创建bucket
		    /*$result = $ossClient->createBucket($bucket);
			dump($result);*/
			//bucket列表
		    $buckets = $ossClient->listBuckets();
			echo "<hr>-----------------------------bucket列表------------------------------<br>";
			dump($buckets);
			/*foreach($buckets->bucketList as $val){
				echo "bucket name: ".$val->name."<hr>";
			}*/
			//对象列表
			$options = array(OssClient::OSS_CHECK_MD5 => true);
			$objects = $ossClient->listObjects($bucket, $options);
			echo "<hr>-----------------------------文件列表------------------------------<br>";
			dump($objects);
			//echo '<pre>';
			//print_r($objects);
			/*foreach($objects->getObjectSummarys() as $val){
				dump($val);
			}*/
			//上传文件
			$file = "C:/Users/Administrator/Desktop/1.php";
			echo "<hr>-----------------------------文件MD5------------------------------<br>";
			echo strtoupper(md5_file($file));
			$object = "wx_notify.php";
			/*$result = $ossClient->uploadFile($bucket, $object, $file, $options);
			dump($result);*/
			echo "<hr>-----------------------------下载链接------------------------------<br>";
			$result = $ossClient->signUrl($bucket, $object, 300);
			dump($result);
			echo "<hr>-----------------------------文件信息------------------------------<br>";
			$result = $ossClient->getObjectMeta($bucket, $object);
			//dump($result);
			$res = array();
			$res['name'] = $object;
			$res['md5'] = str_replace('"', '', $result['etag']);
			$res['size'] = $result['content-length'];
			$res['ranges'] = $result['accept-ranges'];
			$res['last'] = $result['last-modified'];
			dump($res);
		} catch (OssException $e) {
		    print $e->getMessage();
		}
	}
}
