<?php
/**
 * 告警配置model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Model;
use Think\Model;

class AlarmSetModel extends Model
{
	/**
	 * 获取告警配置
	 */
	public function set_by_sid($screen_id){
		$map = array("screen_id"=>$screen_id);
		return $this->where($map)->find();
	}
}
