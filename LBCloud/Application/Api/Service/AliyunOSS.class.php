<?php
/**
 * 阿里云oss存储类
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Service;
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
		//require_once(MODULE_PATH."ORG/oss/autoload.php");
		require_once("./Application/Api/ORG/oss/autoload.php");
		$this->ossId = C("aliyun_oss_id");
		$this->ossSecret = C("aliyun_oss_secret");
		$this->endpoint = C("aliyun_oss_endpoint");
		$this->bucket = C("aliyun_oss_bucket");
		$this->client = new OssClient($this->ossId, $this->ossSecret, $this->endpoint);
	}
	
	/**
	 * 创建bucket
	 * @param $bucket bucket名字
	 * @return boolen
	 */
	public function create_bucket($bucket, $acl="private"){
		try {
			if($bucket){
				$result = $this->client->createBucket($bucket);
				if($result === null){
					return true;
				}else{
					return false;
				}
			}else{
				return false;
			}
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return false;
		}
	}
	
	/**
	 * 存储空间列表
	 * @return array
	 */
	public function bucket_list(){
		try {
			$buckets_list = $this->client->listBuckets();
			$bucket_list = $buckets_list->getBucketList();
			$buckets = array();
			foreach($bucket_list as $val){
				$buckets[] = array(
					'location' => $val->getLocation(),
					'name' => $val->getName(),
					'createDate' => $val->getCreateDate()
				);
			}
			return $buckets;
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return array();
		}
	}
	
	/**
	 * 获取私有存储空间对象下载地址
	 * @param $bucket 存储空间
	 * @param $object 存储对象
	 * @param $timeout 下载地址有效时间（秒）
	 * @return string
	 */
	public function download_uri($bucket, $object, $timeout=1200){
		try {
			$uri = $this->client->signUrl($bucket, $object, $timeout);
			return $uri;
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return "";
		}
	}
	
	/**
	 * 存储文件及目录列表
	 * @param $bucket 存储空间
	 * @param $prefix 存储虚拟目录
	 * @return array
	 */
	public function object_list($bucket = false, $prefix = '', $max = false, $marker = false){
		try {
			$bucket = $bucket ? $bucket : $this->bucket;
			$options = array();
			if($prefix){
				$options['prefix'] = $prefix;
			}
			if($max){
				$options['max-keys'] = $max;
			}
			if($marker){
				$options['marker'] = $marker;
			}
			$object_lists = $this->client->listObjects($bucket, $options);
			$objects = $object_lists->getObjectList();
			$prefixs = $object_lists->getPrefixList();
			$objs = array();
			$pres = array();
			foreach($objects as $obj){
				$objs[] = array(
					'key' => $obj->getKey(),
					'lastModified' => $obj->getLastModified(),
					'eTag' => $obj->getETag(),
					'type' => $obj->getType(),
					'size' => $obj->getSize(),
					'storageClass' => $obj->getStorageClass()
				);
			}
			foreach($prefixs as $val){
				$pres[] = array(
					'prefix' => $val->getPrefix()
				);
			}
			return array('objects'=>$objs, 'prefixs'=>$pres);
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return array();
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
		    //print $e->getMessage();
		    return array();
		}
	}
	
	/**
	 * 根据文件大小获取合理的分片文件大小
	 * @param $filesize 文件总大小
	 * @return int
	 */
	public function part_size($filesize){
		$part_size = 0;
		if($filesize <= C("oss_100K_size")){
			$part_size = C("oss_100K_size");
		}else if($filesize <= C("oss_1M_part_size")){
			$part_size = C("oss_200K_size");
		}else if($filesize <= C("oss_5M_part_size")){
			$part_size = C("oss_1M_part_size");
		}else if($filesize <= C("oss_10M_part_size")){
			$part_size = C("oss_2M_part_size");
		}else if($filesize <= C("oss_50M_part_size")){
			$part_size = C("oss_5M_part_size");
		}else{
			$part_size = C("oss_5M_part_size");
		}
		//testing start
		//$part_size = "3145728";
		//testing end
		return $part_size;
	}
	
	/**
	 * 文件分片
	 * @param $size 文件大小
	 * @param $part 分片大小(5M)
	 * @return array
	 */
	public function generate_upload_part($size, $part = 5242880){
		try {
			$result = $this->client->generateMultiuploadParts($size, $part);
			return $result;
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return array();
		}
	}
	
	/**
	 * 获取分块上传UploadId
	 * @param $subfix 上传文件后缀名
	 * @param $bucket 存储空间
	 * @return array
	 */
	public function get_upload_id($subfix, $bucket=false){
		try {
			$subfix = trim($subfix, '.');
			$object = oss_object($subfix);
			$bucket = $bucket ? $bucket : $this->bucket;
			$uploadId = $this->client->initiateMultipartUpload($bucket, $object);
			$response = array();
			$response['Key']		= $object;
			$response['Bucket']		= $bucket;
			$response['UploadId']	= $uploadId;
			return $response;
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return array();
		}
	}
	
	/**
	 * 获取分片上传签名地址
	 * @param $object 存储对象名称
	 * @param $uploadId 分块上传UploadId
	 * @param $part 分块文件顺序
	 * @param $bucket 存储空间
	 * @param $md5 分块文件md5
	 * @param $timeout 签名过期时间
	 * @return string
	 */
	public function upload_part_sign($object, $uploadId, $part=1, $bucket=false, $md5=false, $timeout=1200){
		try {
			$callback_uri	= ''; // "http://oss-demo.aliyuncs.com:23450";
			$bucket = $bucket ? $bucket : $this->bucket;
			$request_uri	= "http://{$bucket}.".substr($this->endpoint, 7);
			$options = array(
				'partNumber'	=> $part,
				'Content-Type'	=> 'application/octet-stream',
				'uploadId'		=> $uploadId
			);
			if ($md5) {
				$options['Content-Md5'] = $md5;
			}
			$sign_uri = $this->client->signUrl($bucket, $object, $timeout, "PUT", $options);
			$sign_uri = parse_url($sign_uri);
			
			parse_str($sign_uri['query'], $query);
			$query['uploadId'] = $uploadId;
			$request_uri = "{$request_uri}/{$object}?".http_build_query($query);
			return $request_uri;
			/*
			
			$response = array();
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
		    //print $e->getMessage();
		    return "";
		}
	}

	/**
	 * 上传文件签名地址
	 * @param $object 存储对象名称
	 * @param $bucket 存储空间
	 * @param $timeout 签名过期时间
	 */
	public function upload_sign_uri($object, $bucket=false, $timeout=1200){
		try {
			$bucket = $bucket ? $bucket : $this->bucket;
			$options = array(
				'Content-Type'	=> 'application/octet-stream'
			);
			$sign_uri = $this->client->signUrl($bucket, $object, $timeout, "PUT", $options);
			return $sign_uri;
		} catch (OssException $e) {
		   // print $e->getMessage();
		   return "";
		}
	}
	
	/**
	 * 分片上传完成合并文件
	 * @param $object 存储对象名称
	 * @param $uploadId 分块上传UploadId
	 * @param $parts 分块文件顺序及md5
	 * 				array(
	 *					array('PartNumber' => $PartNumber,'ETag' => $eTag),
	 *					array('PartNumber' => $PartNumber,'ETag' => $eTag)
	 *				)
	 * @return boolen
	 */
	public function complete_upload($object, $uploadId, $parts, $bucket=false){
		try {
			$bucket = $bucket ? $bucket : $this->bucket;
			$this->client->completeMultipartUpload($bucket, $object, $uploadId, $parts);
			return true;
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return false;
		}
	}
	
	/**
	 * 中止分片上传
	 * @param $object 存储对象名称
	 * @param $uploadId 分块上传UploadId
	 * @return boolen
	 */
	public function abort_upload($object, $uploadId, $bucket=false){
		try {
			$bucket = $bucket ? $bucket : $this->bucket;
			$this->client->abortMultipartUpload($bucket, $object, $uploadId);
			return true;
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return false;
		}
	}
	
	/**
	 * 未完成的分片上传列表
	 * @return array
	 */
	public function upload_part_list($bucket=false){
		try {
			$options = array(
		        'max-uploads' => 100,
		        'key-marker' => '',
		        'prefix' => '',
		        'upload-id-marker' => ''
		    );
			$bucket = $bucket ? $bucket : $this->bucket;
	        $listMultipartUploadInfo = $this->client->listMultipartUploads($bucket, $options);
			$listUploadInfo = $listMultipartUploadInfo->getUploads();
			$uploads = array();
			foreach($listUploadInfo as $val){
				$uploads[] = array(
					'key' => $val->getKey(),
					'uploadId' => $val->getUploadId(),
					'initiated' => $val->getInitiated()
				);
			}
			return $uploads;
	    } catch (OssException $e) {
	        //printf($e->getMessage() . "\n");
	        return array();
	    }
	}
	
	/**
	 * 分片上传已成功上传的part
	 * @param $object 存储对象
	 * @param $uploadId 分块上传UploadId
	 * @param $bucket 存储空间
	 * @return array
	 */
	public function part_list($object, $uploadId, $bucket=false){
		try {
			$bucket = $bucket ? $bucket : $this->bucket;
			$response = $this->client->listParts($bucket, $object, $uploadId);
			$listPart = $response->getListPart();
			$parts = array();
			foreach($listPart as $val){
				$parts[] = array(
					'partNumber' => $val->getPartNumber(),
					'lastModified' => $val->getLastModified(),
					'eTag' => $val->getETag(),
					'size' => $val->getSize()
				);
			}
			return $parts;
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return array();
		}
	}
	
	/**
	 * 文件是否存在
	 * @param $object 存储对象
	 * @param $bucket 存储空间
	 * @return boolen
	 */
	public function object_exists($object, $bucket=false){
		try {
			$bucket = $bucket ? $bucket : $this->bucket;
			$response = $this->client->doesObjectExist($bucket, $object);
			return $response;
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return false;
		}
	}
	
	/**
	 * 删除存储文件
	 * @param $object 存储对象
	 * @param $bucket 存储空间
	 * @return boolen
	 */
	public function delete_object($object, $bucket=false){
		try {
			$bucket = $bucket ? $bucket : $this->bucket;
			$response = $this->client->deleteObject($bucket, $object);
			//return $response;
			return true;
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return false;
		}
	}
	
	/**
	 * 删除存储文件列表
	 * @param $objects 存储对象
	 * @param $bucket 存储空间
	 * @return boolen
	 */
	public function delete_objects($objects, $bucket=false){
		try {
			$bucket = $bucket ? $bucket : $this->bucket;
			$response = $this->client->deleteObjects($bucket, $objects);
			//return $response;
			return true;
		} catch (OssException $e) {
		    //print $e->getMessage();
		    return false;
		}
	}
}
