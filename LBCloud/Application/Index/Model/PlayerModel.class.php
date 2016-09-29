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
	 * 屏幕绑定id
	 */
	public function bind_id(){
		$id = random_string(8);
		$id_list = $this
					->where('bind_id != ""')
					->getField("bind_id", true);
		while(in_array($id, $id_list)){
			$id = random_string(8);
		}
		return $id;
	}
	
	/**
	 * 屏幕绑定key
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
