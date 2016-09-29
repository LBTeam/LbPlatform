<?php
/**
 * 播放器model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Model;
use Think\Model;

class PlayerModel extends Model
{
	/**
	 * 根据绑定参数获取播放器详情
	 */
	public function player_by_bind($id, $key, $field="*"){
		$map = array();
		$map['bind_id']	= $id;
		$map['bind_key'] = $key;
		return $this->field($field)->where($map)->find();
	}
}
