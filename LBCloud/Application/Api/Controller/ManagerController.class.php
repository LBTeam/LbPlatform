<?php
/**
 * 编辑端接口控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Controller;
use Api\Service\AliyunOSS;

class ManagerController extends CommonController
{
	public function index(){
		$request = file_get_contents('php://input');
		$token = I("request.token");
		
		$proObj = json_decode($request);
		$proId = $proObj->Id;
		$param = $proObj->Para;
		
		//testing start
		$proId = '01003';
		//testing end
		
		$response = array();
		
		if(empty($proId)){
			$response = array("err_code"=>"10001", "msg"=>"内部协议号错误~!");
		}
		
		switch($proId){
			case '01001':
				//登录
				break;
			case '01002':
				//刷新token
				break;
			case '01003':
				//媒体文件分片
				$AliyunOSS = new AliyunOSS();
				$total = $param->total;
				$part = $param->part;
				//testing start
				$total = 102682360;
				$part = 1048576;
				//testing end
				$result = $AliyunOSS->generate_upload_part($total, $part);
				$response = array("err_code"=>"00000", "msg"=>"ok", 'data'=>$result);
				break;
			case '01004':
				//获取上传UploadId
				break;
			case '01005':
				//获取分片上传签名地址
				break;
			case '01006':
				//完成上传
				break;
			case '01007':
				//中止上传
				break;
			case '01008':
				//查询媒体文件是否存在
				break;
			case '01009':
				//获取播放方案列表
				break;
			case '01010':
				//获取播放方案详情
				break;
			case '01011':
				//获取媒体列表
				break;
			case '01012':
				//获取媒体详情
				break;
			case '01013':
				$uid = $param->UserId;
				//testing start
				$uid = 1;
				//testing end
				$result = self::_get_screen_list($uid);
				$response = array("err_code"=>"00000", "msg"=>"ok", 'data'=>$result);
				//获取终端列表
				break;
			case '01014':
				//发布播放方案
				break;
			case '01015':
				//备份播放方案
				break;
			default:
				//登录
				break;
		}
		$this->ajaxReturn($response);
	}

	/**
	 * 获取终端列表
	 */
	protected function _get_screen_list($user_id){
		$screen_model = D("Screen");
		$result = $screen_model->user_all_screen($user_id);
		return $result;
	}
}
