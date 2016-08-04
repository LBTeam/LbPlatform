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
	private $ossId;
	private $ossSecret;
	private $endpoint;
	private $bucket;
	public function __construct(){
		require_once(APP_PATH."/api/org/oss/autoload.php");
		$this->ossId = config("aliyun_oss_id");
		$this->ossSecret = config("aliyun_oss_secret");
		$this->endpoint = config("aliyun_oss_endpoint");
		$this->bucket = config("aliyun_oss_bucket");
		$this->client = new OssClient($this->ossId, $this->ossSecret, $this->endpoint);
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
			$response = $this->client->getObjectMeta($bucket, $object);
			$result = array();
			$result['name'] = $object;
			$result['md5'] = str_replace('"', '', $response['etag']);
			$result['size'] = $response['content-length'];
			$result['ranges'] = $response['accept-ranges'];
			$result['last'] = $response['last-modified'];
			return $result;
		} catch (OssException $e) {
		    print $e->getMessage();
		}
	}
	
	/**
	 * 获取分块上传UploadId
	 * @param $subfix 上传文件后缀名
	 * @return array
	 */
	public function get_upload_id($subfix){
		try {
			$subfix = trim($subfix, '.');
			$object = uniqid() . (!empty($subfix) ? ".$subfix": '');
			$uploadId = $this->client->initiateMultipartUpload($this->bucket, $object);
			$response = [];
			$response['Key']		= $object;
			$response['Bucket']		= $this->bucket;
			$response['UploadId']	= $uploadId;
			return $response;
		} catch (OssException $e) {
		    print $e->getMessage();
		}
	}
	
	/**
	 * 获取分片上传签名地址
	 * @param $object 存储对象名称
	 * @param $uploadId 分块上传UploadId
	 * @param $part 分块文件顺序
	 * @param $md5 分块文件md5
	 * @param $timeout 签名过期时间
	 * @return string
	 */
	public function upload_part_sign($object, $uploadId, $part=1, $md5=false, $timeout=300){
		try {
			$callback_uri	= ''; // "http://oss-demo.aliyuncs.com:23450";
			$request_uri	= "http://{$this->bucket}.".substr($this->endpoint, 7);
			$ossClient = new OssClient($this->ossId, $this->ossSecret, $this->endpoint);
			$options = [
				'partNumber'	=> $part,
				'Content-Type'	=> 'application/octet-stream',
				'UploadId'		=> $uploadId
			];
			if ($md5) {
				$options['Content-Md5'] = $md5;
			}
			$sign_uri = $this->client->signUrl($this->bucket, $object, $timeout, "PUT", $options);
			return $sign_uri;
			/*
			$sign_uri = parse_url($sign_uri);
			$response = [];
			$response['OSSAccessKeyId']  = $this->ossId;
			$response['PostServer']      = $request_uri;
			$response['Endpoint']        = $this->endpoint;
			$response['PartNumber']      = $part;
			//这个参数是设置用户上传指定的前缀
			$response['Key']             = $object;
			$response['Bucket']          = $this->bucket;
			$response['UploadId']        = $uploadId;
			parse_str($sign_uri['query'], $query);
			foreach ($query as $k => $v) {
				$response[ucfirst(rawurldecode($k))] = rawurlencode($v);
			}
			return $response;
			*/
		} catch (OssException $e) {
		    print $e->getMessage();
		}
	}
	
	/**
	 * 分片上传完成合并文件
	 */
	public function complete_upload($object, $uploadId, $parts){
		
	}
}
