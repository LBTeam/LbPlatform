<?php
/**
 * 监控数据model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class AlarmModel extends Model
{
	/**
	 * 根据屏幕Id获取监控数据
	 */
	public function alarm_by_sid($screen_id, $type=0, $field="*"){
		$map = array();
		$map['screen_id'] = $screen_id;
		$map['type'] = $type;
		return $this->field($field)->where($map)->find();
	}
}
