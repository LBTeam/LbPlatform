<?php
/**
 * 播放器model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class PlayerModel extends Model
{
	/**
	 * 屏幕唯一key
	 */
	public function bind_key(){
		$key = random_string(16);
		$key_list = $this
					->where('bind_key != ""')
					->getField("bind_key", true);
		while(in_array($key, $key_list)){
			$key = random_string(16);
		}
		return $key;
	}
	
	/**
	 * 播放器信息
	 */
	public function player_by_id($id){
		if($id){
			return $this->where("id={$id}")->find();
		}
		return false;
	}
}
