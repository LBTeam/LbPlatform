<?php
namespace app\api\controller;
use think\Loader;
use OSS\OssClient;
use OSS\Core\OssException;

class Index
{
    public function index()
    {
    	//Loader::import('@.org.oss.autoload', EXTEND_PATH);
    	require_once(APP_PATH."/api/org/oss/autoload.php");
		$accessKeyId = "f1mcwCSSqB9tIY57";
		$accessKeySecret = "7aSeEDlyyV5pIddw49FhLXFMVvG9UK";
		$endpoint = "http://oss-cn-shenzhen.aliyuncs.com";
		$bucket = "lb-player-test";
		
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
