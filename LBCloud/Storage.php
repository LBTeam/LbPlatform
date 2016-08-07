<?php
namespace app\oss\controller;

use OSS\OssClient;
use OSS\Core\OssException;
use OSS\Http\RequestCore;
use OSS\Http\ResponseCore;
use OSS\Core\OssUtil;
use think\Response;
use think\Request as R;
use think\Config as C;

\think\Loader::import('aliyun_sts_php_sdk.aliyun-php-sdk-core.Config', EXTEND_PATH);	
use Sts\Request\V20150401 as Sts;

class Storage
{
	// POST上传签名
	public function signature($app='', $user='', $custom_fields = array())
	{
		$accessKeyId     = C::get('custom.oss_access_key'); // '6MKOqxGiGU4AUk44';
		$accessKeySecret = C::get('custom.oss_access_secret'); //'ufu7nS8kS59awNihtjSonMETLI0KLy';
		$endpoint        = C::get('custom.oss_endpoint'); // 'oss-cn-hangzhou.aliyuncs.com' ;
		$bucket          = C::get('custom.oss_bucket'); // 'post-test';
		$callbackUrl     = C::get('custom.oss_callback'); // "http://oss-demo.aliyuncs.com:23450";
		$server          = "http://$bucket.$endpoint";
		$dir 			 = '';
		$callbackVar     = '';
		if (!empty($app))
			$dir .= "$app/";
		if (!empty($user))
			$dir .= "$user/";
		if (!empty($custom_fields)) {
			foreach ($custom_fields as $v) {
				list(,$val) = explode(':',$v);
				$callbackVar .= "&$val=\${x:$val}";
			}
		}
		// 回调设置
		$callback = [
					 'callbackUrl'=>$callbackUrl, 
					 'callbackBody'=>'filename=${object}&size=${size}&mimeType=${mimeType}&height=${imageInfo.height}&width=${imageInfo.width}'.$callbackVar, 
					 'callbackBodyType'=>"application/x-www-form-urlencoded"];
					 
		// $callback_string      = json_encode($callback_param);
		$base64_callback_body = base64_encode(json_encode($callback));
		$now        = time();
		$expire     = 60; //设置该policy超时时间是60s. 即这个policy过了这个有效时间，将不能访问
		$end        = $now + $expire;
		$expiration = gmt_iso8601($end);

		// 上传规则设置
		//最大文件大小.用户可以自己设置
		$condition = ['content-length-range',1, 1048576000];
		$conditions[] = $condition; 

		//表示用户上传的数据,必须是以$dir开始, 不然上传会失败,这一步不是必须项,只是为了安全起见,防止用户通过policy上传到别人的目录
		$start = ['starts-with', '$key', $dir];
		$conditions[] = $start;
		$arr = ['expiration'=>$expiration, 'conditions'=>$conditions];
		//echo json_encode($arr);
		//return;
		$policy         = json_encode($arr);
		$base64_policy  = base64_encode($policy);
		$string_to_sign = $base64_policy;
		$signature      = base64_encode(hash_hmac('sha1', $string_to_sign, $accessKeySecret, true));

		$response = [];
		$response['OSSAccessKeyId']  = $accessKeyId;
		$response['PostServer']      = $server;
		$response['Endpoint']        = $endpoint;
		$response['Policy']          = $base64_policy;
		$response['Signature']       = $signature;
		$response['Expires']         = $end;
		$response['Callback']        = $base64_callback_body;
		//这个参数是设置用户上传指定的前缀
		$response['KeyPrefix']       = $dir;
		$response['Bucket']          = $bucket;
		return Response::create(['status'=>'0', 'msg'=>'ok', 'data'=>$response], 'json')->code(200);
	}
	
	
	// !文件列表(弃)
	public function lists($app='',$user='')
	{
		$accessKeyId     = C::get('custom.oss_access_key'); // '6MKOqxGiGU4AUk44';
		$accessKeySecret = C::get('custom.oss_access_secret'); //'ufu7nS8kS59awNihtjSonMETLI0KLy';
		$endpoint        = C::get('custom.oss_endpoint'); // 'oss-cn-hangzhou.aliyuncs.com' ;
		$bucket          = C::get('custom.oss_bucket'); // 'post-test';
		$callbackUrl     = C::get('custom.oss_callback'); // "http://oss-demo.aliyuncs.com:23450";
		
		
		$ossClient = new OssClient($accessKeyId, $accessKeySecret, $endpoint);
		
		$prefix = '';
		if (!empty($app))
			$prefix .= "$app/";
		if (!empty($user))
			$prefix .= "$user/";

		$delimiter = '/';
		$nextMarker = '';
		$maxkeys = 1000;
		$options = array(
			'delimiter' => '',
			'prefix' => $prefix,
			'max-keys' => $maxkeys,
			'marker' => $nextMarker,
		);
		try {
			$listObjectInfo = $ossClient->listObjects($bucket, $options);
		} catch (OssException $e) {
			printf(__FUNCTION__ . ": FAILED\n");
			printf($e->getMessage() . "\n");
			return;
		}
		print(__FUNCTION__ . ": OK" . "\n");
		$objectList = $listObjectInfo->getObjectList(); // 文件列表
		$prefixList = $listObjectInfo->getPrefixList(); // 目录列表
		if (!empty($objectList)) {
			print("objectList:\n");
			foreach ($objectList as $objectInfo) {
				print($objectInfo->getKey() . "\n");
			}
		}
		if (!empty($prefixList)) {
			print("prefixList: \n");
			foreach ($prefixList as $prefixInfo) {
				print($prefixInfo->getPrefix() . "\n");
			}
		}
	}
	
	// ?回调(选用)
	public function callback()
	{
		// 1.获取OSS的签名header和公钥url header
		$authorizationBase64 = "";
		$pubKeyUrlBase64 = "";
		/*
		 * 注意：如果要使用HTTP_AUTHORIZATION头，你需要先在apache或者nginx中设置rewrite，以apache为例，修改
		 * 配置文件/etc/httpd/conf/httpd.conf(以你的apache安装路径为准)，在DirectoryIndex index.php这行下面增加以下两行
			RewriteEngine On
			RewriteRule .* - [env=HTTP_AUTHORIZATION:%{HTTP:Authorization},last]
		 * */
		if (isset($_SERVER['HTTP_AUTHORIZATION']))
		{
			$authorizationBase64 = $_SERVER['HTTP_AUTHORIZATION'];
		}
		if (isset($_SERVER['HTTP_X_OSS_PUB_KEY_URL']))
		{
			$pubKeyUrlBase64 = $_SERVER['HTTP_X_OSS_PUB_KEY_URL'];
		}

		if ($authorizationBase64 == '' || $pubKeyUrlBase64 == '')
		{
			header("http/1.1 403 Forbidden");
			exit();
		}

		// 2.获取OSS的签名
		$authorization = base64_decode($authorizationBase64);

		// 3.获取公钥
		$pubKeyUrl = base64_decode($pubKeyUrlBase64);
		$ch = curl_init();
		curl_setopt($ch, CURLOPT_URL, $pubKeyUrl);
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
		curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, 10);
		$pubKey = curl_exec($ch);
		if ($pubKey == "")
		{
			//header("http/1.1 403 Forbidden");
			exit();
		}

		// 4.获取回调body
		$body = file_get_contents('php://input');

		// 5.拼接待签名字符串
		$authStr = '';
		$path = $_SERVER['REQUEST_URI'];
		$pos = strpos($path, '?');
		if ($pos === false)
		{
			$authStr = urldecode($path)."\n".$body;
		}
		else
		{
			$authStr = urldecode(substr($path, 0, $pos)).substr($path, $pos, strlen($path) - $pos)."\n".$body;
		}

		// 6.验证签名
		$ok = openssl_verify($authStr, $authorization, $pubKey, OPENSSL_ALGO_MD5);
		if ($ok == 1)
		{
			header("Content-Type: application/json");
			$data = array("Status"=>"Ok");
			echo json_encode($data);
		}
		else
		{
			header("http/1.1 403 Forbidden");
			exit();
		}
		
	}
	
	
	// 生成临时访问token
	// RAM和STS授权策略（policy）配置 https://help.aliyun.com/document_detail/31867.html?spm=5176.87240.400427.27.rqcCPK
	public function token($app='', $user ='')
	{
		$accessKeyId     = C::get('custom.oss_access_key'); // '6MKOqxGiGU4AUk44';
		$accessKeySecret = C::get('custom.oss_access_secret'); //'ufu7nS8kS59awNihtjSonMETLI0KLy';
		$endpoint        = C::get('custom.oss_endpoint'); // 'oss-cn-hangzhou.aliyuncs.com' ;
		$bucket          = C::get('custom.oss_bucket'); // 'post-test';
		$callbackUrl     = C::get('custom.oss_callback'); // "http://oss-demo.aliyuncs.com:23450";
				
		$prefix = '';
		if (!empty($app))
			$prefix .= "$app/";
		if (!empty($user))
			$prefix .= "$user/";

		// 在扮演角色(AssumeRole)时，可以附加一个授权策略，进一步限制角色的权限；
		// 详情请参考《RAM使用指南》
		/*		
		// sample
		{
			"Version": "1",
			"Statement": [
				{
					"Effect": "Allow",
					"Action": "ecs:Describe*",
					"Resource": "acs:ecs:cn-hangzhou:*:*"
				},
				{
					"Effect": "Allow",
					"Action": [
						"oss:ListObjects",
						"oss:GetObject"
					],
					"Resource": [
						"acs:oss:*:*:mybucket",
						"acs:oss:*:*:mybucket/*"
					],
					"Condition":{
						"IpAddress": {
							"acs:SourceIp": ["42.120.88.10", "42.120.66.0/24"]
						}
					}
				}
			]
		}
		*/
		// 此授权策略表示读取所有OSS的只读权限
		$policy=<<<POLICY
		{
		  "Statement": [
			{
			  "Action": [
				"oss:Get*",
				"oss:List*",
                "oss:*Object*",
				"oss:*Upload*"
			  ],
			  "Effect": "Allow",
			  "Resource": "acs:oss:*:*:$bucket/{$prefix}*"
			}
		  ],
		  "Version": "1"
		}
POLICY;

		try {
			// 你需要操作的资源所在的region，STS服务目前只有杭州节点可以签发Token，签发出的Token在所有Region都可用
			// 只允许子用户使用角色
			$iClientProfile = \DefaultProfile::getProfile("cn-hangzhou", $accessKeyId, $accessKeySecret);
			$client = new \DefaultAcsClient($iClientProfile);
			
			// 角色资源描述符，在RAM的控制台的资源详情页上可以获取
			$roleArn = C::get('custom.oss_role_arn'); //"<role-arn>";
			$request = new Sts\AssumeRoleRequest();
			// RoleSessionName即临时身份的会话名称，用于区分不同的临时身份
			// 您可以使用您的客户的ID作为会话名称
			$request->setRoleSessionName($user); //client_name
			$request->setRoleArn($roleArn);
			$request->setPolicy($policy);
			$request->setDurationSeconds(3600);
			$response = $client->getAcsResponse($request);
			//dump($response );
			return Response::create(['status'=>'0', 'msg'=>'ok', 'data'=>$response], 'json')->code(200);
		} catch (\ClientException $e) {
			return Response::create(['status'=>'1','msg'=> $e->getMessage()], 'json')->code(200);
		} finally {
			return Response::create(['status'=>'1','msg'=> $e->getMessage()], 'json')->code(200);
		}
		
	}
	
	/**
	 * 生成GetObject的签名url,主要用于私有权限下的读访问控制
	 *
	 * @param $ossClient OssClient OSSClient实例
	 * @param $bucket string bucket名称
	 * @return null
	 */
	public function getSignedUrlForGettingObject($object ='test/test-signature-test-upload-and-download.txt')
	{
		$accessKeyId     = C::get('custom.oss_access_key'); // '6MKOqxGiGU4AUk44';
		$accessKeySecret = C::get('custom.oss_access_secret'); //'ufu7nS8kS59awNihtjSonMETLI0KLy';
		$endpoint        = C::get('custom.oss_endpoint'); // 'oss-cn-hangzhou.aliyuncs.com' ;
		$bucket          = C::get('custom.oss_bucket'); // 'post-test';
		$callbackUrl     = C::get('custom.oss_callback'); // "http://oss-demo.aliyuncs.com:23450";
		
		$timeout = 3600; // URL的有效期是3600秒
		try {
		    $ossClient = new OssClient($accessKeyId, $accessKeySecret, $endpoint);
		    $signedUrl = $ossClient->signUrl($bucket, $object, $timeout);
		} catch (OssException $e) {
			return Response::create(['status'=>'1','msg'=> $e->getMessage()], 'json')->code(200);
		}

		$response = ['SignedUrl' => $signedUrl];
		return Response::create(['status'=>'0', 'msg'=>'ok', 'data'=>$response], 'json')->code(200);
		
	}
	
	/**
	 * 生成PutObject的签名url,主要用于私有权限下的写访问控制
	 *
	 * @param OssClient $ossClient OSSClient实例
	 * @param string $bucket bucket名称
	 * @return null
	 * @throws OssException
	 */
	public function getSignedUrlForPuttingObject($app = '', $user = '', $subfix='', $params = [] )
	{
		$accessKeyId     = C::get('custom.oss_access_key'); // '6MKOqxGiGU4AUk44';
		$accessKeySecret = C::get('custom.oss_access_secret'); //'ufu7nS8kS59awNihtjSonMETLI0KLy';
		$endpoint        = C::get('custom.oss_endpoint'); // 'oss-cn-hangzhou.aliyuncs.com' ;
		$bucket          = C::get('custom.oss_bucket'); // 'post-test';
		$callbackUrl     = C::get('custom.oss_callback'); // "http://oss-demo.aliyuncs.com:23450";
		
		$subfix = trim($subfix, '.');
		$dir = '';
		if (!empty($app))
			$dir .= "$app/";
		if (!empty($user))
			$dir .= "$user/";
		$object = $dir . uniqid() . (!empty($subfix) ? ".$subfix": ''); // "test/multipart-test.txt";
		$timeout = 60;  // 60s过期
		$options = NULL;
		
		$ossClient = new OssClient($accessKeyId, $accessKeySecret, $endpoint);
		
		$callbackValFields = '';
		if (!empty($params)) {
			foreach ($params as $k=>$v) {
				list(,$val) = explode(':',$k);
				$callbackValFields .= "&$val=\${x:$val}";
			}
		}
		$callback = array(
					 'callbackUrl'=>$callbackUrl, 
					 'callbackBody'=>'filename=${object}&size=${size}&mimeType=${mimeType}&height=${imageInfo.height}&width=${imageInfo.width}'. $callbackValFields, 
					 'callbackBodyType'=>"application/x-www-form-urlencoded");
		
		// callback 参与签名		 
		$base64Callback    = base64_encode(json_encode($callback));
		$options[OssClient::OSS_SUB_RESOURCE] = 'callback=' . rawurlencode($base64Callback);
		
		// callback-val(自定义参数) 不参与签名(oss文档有误)
		$base64CallbackVar = base64_encode(json_encode((array)$params));
		//if (!empty($params)) {
			//$options[OssClient::OSS_SUB_RESOURCE] .= '&' . rawurlencode('callback-val') . '=' .rawurlencode($base64CallbackVar);
		//}
		try {
			$signedUrl = $ossClient->signUrl($bucket, $object, $timeout, "PUT", $options);
			$signedUrl = $signedUrl. (!empty($params)? '&callback-val=' .rawurlencode($base64CallbackVar) : '');
		} catch (OssException $e) {
			return Response::create(['status'=>'1','msg'=> $e->getMessage()], 'json')->code(200);
		}
		
		$response = ['SignedUrl' => $signedUrl];
		return Response::create(['status'=>'0', 'msg'=>'ok', 'data'=>$response], 'json')->code(200);
		
		/**
		// test
		print(__FUNCTION__ . ": signedUrl: " . $signedUrl. "\n");
		$content = file_get_contents(__FILE__);
		$request = new RequestCore($signedUrl);
		$request->set_method('PUT');
		$request->add_header('Content-Type', '');
		$request->add_header('Content-Length', strlen($content));
		$request->set_body($content);
		// dump($request);
		$request->set_proxy('127.0.0.1:8888');
		$request->send_request();
		$res = new ResponseCore($request->get_response_header(),$request->get_response_body(), $request->get_response_code());
		if ($res->isOK()) {
			print(__FUNCTION__ . ": OK" . "\n");
		} else {
			print(__FUNCTION__ . ": FAILED" . "\n");
		};
		*/
	}
	

	
	
	// 获取分片上传uploadId
	public function getMultipartUploadId($app='', $user='', $subfix='')
	{
		$accessKeyId     = C::get('custom.oss_access_key'); // '6MKOqxGiGU4AUk44';
		$accessKeySecret = C::get('custom.oss_access_secret'); //'ufu7nS8kS59awNihtjSonMETLI0KLy';
		$endpoint        = C::get('custom.oss_endpoint'); // 'oss-cn-hangzhou.aliyuncs.com' ;
		$bucket          = C::get('custom.oss_bucket'); // 'post-test';
		$callbackUrl     = C::get('custom.oss_callback'); // "http://oss-demo.aliyuncs.com:23450";
		
		$server          = "http://$bucket.$endpoint";
		$subfix = trim($subfix, '.');
		
		$ossClient = new OssClient($accessKeyId, $accessKeySecret, $endpoint);
		
		$dir = '';
		if (!empty($app))
			$dir .= "$app/";
		if (!empty($user))
			$dir .= "$user/";
		$object = $dir . uniqid() . (!empty($subfix) ? ".$subfix": ''); // "test/multipart-test.txt";
		
		/**
		 *  step 1. 初始化一个分块上传事件, 也就是初始化上传Multipart, 获取upload id
		 */
		try {
			$uploadId = $ossClient->initiateMultipartUpload($bucket, $object);
		} catch (OssException $e) {
			return Response::create(['status'=>'1','msg'=> $e->getMessage()], 'json')->code(200);
			// return $e->getMessage();
		}
		
		$response = [];
		$response['OSSAccessKeyId']  = $accessKeyId;
		$response['PostServer']      = $server;
		$response['Endpoint']        = $endpoint;
		//这个参数是设置用户上传指定的前缀
		$response['Key']             = $object;
		$response['Bucket']          = $bucket;
		$response['UploadId']        = $uploadId;
		return Response::create(['status'=>'0','msg'=>'ok','data'=>$response], 'json')->code(200);
	}
	
	
	// 分片上传签名(每块都签名)
	public function getUploadPartSign($object='', $uploadId='',$partNumber=1, $contentMd5=false)
	{		
		$accessKeyId     = C::get('custom.oss_access_key'); // '6MKOqxGiGU4AUk44';
		$accessKeySecret = C::get('custom.oss_access_secret'); //'ufu7nS8kS59awNihtjSonMETLI0KLy';
		$endpoint        = C::get('custom.oss_endpoint'); // 'oss-cn-hangzhou.aliyuncs.com' ;
		$bucket          = C::get('custom.oss_bucket'); // 'post-test';
		$callbackUrl     = C::get('custom.oss_callback'); // "http://oss-demo.aliyuncs.com:23450";
		
		$server          = "http://$bucket.$endpoint";
		$ossClient = new OssClient($accessKeyId, $accessKeySecret, $endpoint);
		$timeout = 60; // 60s过期

		
		$upOptions = [
			$ossClient::OSS_PART_NUM     => $partNumber,
			$ossClient::OSS_CONTENT_TYPE => 'application/octet-stream',
			$ossClient::OSS_UPLOAD_ID    => $uploadId,
		];
		if ($contentMd5) {
			$upOptions[$ossClient::OSS_CONTENT_MD5] = $contentMd5;
		}
		//2. 将每一分片上传到OSS
		try {
			$signedUrl = $ossClient->signUrl($bucket, $object, $timeout, "PUT", $upOptions);
		} catch (OssException $e) {
			return Response::create(['status'=>'1','msg'=> $e->getMessage()], 'json')->code(200);
		}
		$signedUrl = parse_url($signedUrl);
		$response = [];
		$response['OSSAccessKeyId']  = $accessKeyId;
		$response['PostServer']      = $server;
		$response['Endpoint']        = $endpoint;
		$response['PartNumber']      = $partNumber;
		//这个参数是设置用户上传指定的前缀
		$response['Key']             = $object;
		$response['Bucket']          = $bucket;
		$response['UploadId']        = $uploadId;
		parse_str($signedUrl['query'], $query);
		foreach ($query as $k => $v) {
			$response[ucfirst(rawurldecode($k))] = rawurlencode($v);
		}
		return Response::create(['status'=>'0','msg'=>'ok','data'=>$response], 'json')->code(200);
		// return Response::create($response, 'json')->code(200);
	}
	
	
	// 分片上传完成合并
	// $uploadParts[] = array('PartNumber' => $PartNumber,'ETag' => $eTag);
	public function completeMultipartUpload($object='', $uploadId='',$uploadParts=[], $params = [])
	{
		$accessKeyId     = C::get('custom.oss_access_key'); // '6MKOqxGiGU4AUk44';
		$accessKeySecret = C::get('custom.oss_access_secret'); //'ufu7nS8kS59awNihtjSonMETLI0KLy';
		$endpoint        = C::get('custom.oss_endpoint'); // 'oss-cn-hangzhou.aliyuncs.com' ;
		$bucket          = C::get('custom.oss_bucket'); // 'post-test';
		$callbackUrl     = C::get('custom.oss_callback'); // "http://oss-demo.aliyuncs.com:23450";
		
		$server          = "http://$bucket.$endpoint";
		$ossClient       = new OssClient($accessKeyId, $accessKeySecret, $endpoint);
	    $option          = null;
		// $uploadParts[] = ['PartNumber' => $partNumber, 'ETag' => $eTag];
		$callbackValFields = '';
		if (!empty($params)) {
			foreach ($params as $k=>$v) {
				list(,$val) = explode(':',$k);
				$callbackValFields .= "&$val=\${x:$val}";
			}
		}
		$callback = array(
					 'callbackUrl'=>$callbackUrl, 
					 'callbackBody'=>'filename=${object}&size=${size}&mimeType=${mimeType}&height=${imageInfo.height}&width=${imageInfo.width}'. $callbackValFields, 
					 'callbackBodyType'=>"application/x-www-form-urlencoded");
		
		// callback 参与签名		 
		$base64Callback    = base64_encode(json_encode($callback));
		$options[OssClient::OSS_SUB_RESOURCE] = 'callback=' . rawurlencode($base64Callback);
		
		// callback-val(自定义参数) 不参与签名(oss文档有误)
		$base64CallbackVar = base64_encode(json_encode((array)$params));
		if (!empty($params))
			$options[OssClient::OSS_QUERY_STRING] = ['callback-val' => $base64CallbackVar];
		//if (!empty($params)) {
			//$options[OssClient::OSS_SUB_RESOURCE] .= '&' . rawurlencode('callback-val') . '=' .rawurlencode($base64CallbackVar);
		//}
		
		try {
			$ossClient->completeMultipartUpload($bucket, $object, $uploadId, $uploadParts, $options);
		} catch (OssException $e) {
			return Response::create(['status'=>'1','msg'=>$e->getMessage()], 'json')->code(200);
		}
		
		return Response::create(['status'=>'0','msg'=>'ok'], 'json')->code(200);
		
	}
	
	// form表单上传测试
	public function testPostUpload()
	{
		$view = new \think\View([],['__PUBLIC__'=>'/tp5/public','__ROOT__' => '/tp5/']); 
		return $view->fetch(); 
	}
	
	// 测试分片上传
	public function testMultipart($app='',$user='',$md5content = '')
	{
		$accessKeyId     = C::get('custom.oss_access_key'); // '6MKOqxGiGU4AUk44';
		$accessKeySecret = C::get('custom.oss_access_secret'); //'ufu7nS8kS59awNihtjSonMETLI0KLy';
		$endpoint        = C::get('custom.oss_endpoint'); // 'oss-cn-hangzhou.aliyuncs.com' ;
		$bucket          = C::get('custom.oss_bucket'); // 'post-test';
		$callbackUrl     = C::get('custom.oss_callback'); // "http://oss-demo.aliyuncs.com:23450";
		
		$ossClient = new OssClient($accessKeyId, $accessKeySecret, $endpoint);
		$subfix = trim($subfix, '.');
		$dir = '';
		if (!empty($app))
			$dir .= "$app/";
		if (!empty($user))
			$dir .= "$user/";
		$object = $dir . uniqid() . (!empty($subfix) ? ".$subfix": ''); // "test/multipart-test.txt";
		/**
		 *  step 1. 初始化一个分块上传事件, 也就是初始化上传Multipart, 获取upload id
		 */
		try {
			$uploadId = $ossClient->initiateMultipartUpload($bucket, $object);
		} catch (OssException $e) {
			printf(__FUNCTION__ . ": initiateMultipartUpload FAILED\n");
			printf($e->getMessage() . "\n");
			return;
		}
		print(__FUNCTION__ . ": initiateMultipartUpload OK, uploadId:$uploadId" . "\n");
		/*
		 * step 2. 上传分片
		 */
		$partSize = 10 * 1024 * 1024;
		$uploadFile = __FILE__;
		$uploadFileSize = filesize($uploadFile);
		$pieces = $ossClient->generateMultiuploadParts($uploadFileSize, $partSize);
		$responseUploadPart = [];
		$uploadPosition = 0;
		$isCheckMd5 = true;
		foreach ($pieces as $i => $piece) {
			$fromPos = $uploadPosition + (integer)$piece[$ossClient::OSS_SEEK_TO];
			$toPos = (integer)$piece[$ossClient::OSS_LENGTH] + $fromPos - 1;
			$upOptions = [
				$ossClient::OSS_FILE_UPLOAD => $uploadFile,
				$ossClient::OSS_PART_NUM => ($i + 1),
				$ossClient::OSS_SEEK_TO => $fromPos,
				$ossClient::OSS_LENGTH => $toPos - $fromPos + 1,
				$ossClient::OSS_CHECK_MD5 => $isCheckMd5,
			];
			if ($isCheckMd5) {
				$contentMd5 = OssUtil::getMd5SumForFile($uploadFile, $fromPos, $toPos);
				$upOptions[$ossClient::OSS_CONTENT_MD5] = $contentMd5;
			}
			//2. 将每一分片上传到OSS
			try {
				$responseUploadPart[] = $ossClient->uploadPart($bucket, $object, $uploadId, $upOptions);
			} catch (OssException $e) {
				printf(__FUNCTION__ . ": initiateMultipartUpload, uploadPart - part#{$i} FAILED\n");
				printf($e->getMessage() . "\n");
				return;
			}
			printf(__FUNCTION__ . ": initiateMultipartUpload, uploadPart - part#{$i} OK\n");
		}
		$uploadParts = array();
		foreach ($responseUploadPart as $i => $eTag) {
			$uploadParts[] = ['PartNumber' => ($i + 1),'ETag' => $eTag];
		}
		/**
		 * step 3. 完成上传
		 */
		try {
			$ossClient->completeMultipartUpload($bucket, $object, $uploadId, $uploadParts);
		} catch (OssException $e) {
			printf(__FUNCTION__ . ": completeMultipartUpload FAILED\n");
			printf($e->getMessage() . "\n");
			return;
		}
		printf(__FUNCTION__ . ": completeMultipartUpload OK\n");
			
	}
}
