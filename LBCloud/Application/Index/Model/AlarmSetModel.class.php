<?php
/**
 * 告警配置model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class AlarmSetModel extends Model
{
	/**
	 * 更新告警配置
	 */
	public function update_set($data){
		$map = array(
			"screen_id" => $data['screen_id']
		);
		$set_id = $this->where($map)->getField("id");
		if($set_id){
			$data['id'] = $set_id;
			return $this->save($data);
		}else{
			return $this->add($data);
		}
	}
	
	/**
	 * 获取告警配置
	 */
	public function set_by_sid($screen_id){
		$map = array("screen_id"=>$screen_id);
		return $this->where($map)->find();
	}
}
