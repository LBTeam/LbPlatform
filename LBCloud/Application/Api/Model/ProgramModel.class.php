<?php
/**
 * 播放方案model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Model;
use Think\Model;

class ProgramModel extends Model
{
	/**
	 * 检查播放方案是否存在
	 * @param $name 播放方案名称
	 * @param $md5 播放方案md5
	 * @param $user_id 用户ID
	 * @return int|boolen
	 */
	public function program_exists($name, $md5, $user_id){
		$map = array();
		$map['user_id'] = $user_id;
		$map['name'] = $name;
		$map['md5'] = $md5;
		$program_id = $this->where($map)->getField('id');
		if($result){
			return $program_id;
		}else{
			return false;
		}
	}
	
	/**
	 * 播放方案详情
	 * @param $program_id 播放方案ID
	 * @return array
	 */
	public function program_detail($program_id){
		$map = array();
		$map['id'] = $user_id;
		return $this->where($map)->find();
	}
	
	/**
	 * 获取播放方案详情
	 * @param $name 播放方案名称
	 * @param $md5 播放方案md5
	 * @param $user_id 用户ID
	 * @return array
	 */
	public function program_by_name_md5($name, $md5, $user_id){
		$map = array();
		$map['user_id'] = $user_id;
		$map['name'] = $name;
		$map['md5'] = $md5;
		return $this->where($map)->find();
	}
}
