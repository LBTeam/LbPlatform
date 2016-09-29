<?php
/**
 * 播放器参数model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class SettingModel extends Model
{
	protected $tableName = "player_setting";
	
	/**
	 * 根据屏幕ID获取播放器配置
	 */
	public function set_by_sid($screen_id, $field="*"){
		$map = array("id"=>$screen_id);
		return $this->field($field)->where($map)->find();
	}
}
