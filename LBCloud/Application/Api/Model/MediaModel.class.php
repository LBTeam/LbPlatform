<?php
/**
 * 播放媒体model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Model;
use Think\Model;

class MediaModel extends Model
{
	/**
	 * 检查媒体是否存在
	 * @param $name 媒体名称
	 * @param $md5 媒体md5
	 * @param $user_id 用户ID
	 * @return int|boolen
	 */
	public function media_exists($name, $md5, $user_id){
		$map = array();
		$map['user_id'] = $user_id;
		$map['name'] = mysql_escape_string($name);
		$map['md5'] = $md5;
		$media_id = $this->where($map)->getField('id');
		if($media_id){
			return $media_id;
		}else{
			return false;
		}
	}
	
	/**
	 * 媒体详情
	 * @param $media_id 媒体ID
	 * @return array
	 */
	public function media_detail($media_id){
		$map = array();
		$map['id'] = $media_id;
		return $this->where($map)->find();
	}
	
	/**
	 * 获取媒体详情
	 * @param $name 媒体名称
	 * @param $md5 媒体md5
	 * @param $user_id 用户ID
	 * @return array
	 */
	public function media_by_name_md5($name, $md5, $user_id){
		$map = array();
		$map['user_id'] = $user_id;
		$map['name'] = mysql_escape_string($name);
		$map['md5'] = $md5;
		return $this->where($map)->find();
	}
}
