<?php
/**
 * 播放记录model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class RecordModel extends Model
{
	/**
	 * 获取播放记录
	 * @param $uid 用户ID
	 * @param $media 媒体名称（模糊查询）
	 * @param $name 播放器名称（模糊查询）
	 * @return array
	 */
	public function record_by_uid($uid, $media='', $name=''){
		$map = array();
		if($media){
			$map['r.media_name'] = array("LIKE", "%{$media}%");
		}
		if($name){
			$map['p.name'] = array("LIKE", "%{$name}%");
		}
		if(is_administrator()){
			return $this
					->alias("r")
					->field("r.*,p.name,u.email,u.phone")
					->join("player_screen AS s ON r.screen_id = s.id")
					->join("player_user AS u ON s.uid = u.uid")
					->join("player_player AS p ON r.screen_id = p.id")
					->where($map)
					->select();
		}else{
			$user_model = D("User");
			$cfg_model = D("Config");
			$role_model = D("Role");
			$roles = $cfg_model->roles();
			$role_id = $role_model->role_id_by_user($uid);
			if($user_model->is_root($uid, $role_id, $roles)){
				return $this
						->alias("r")
						->field("r.*,p.name,u.email,u.phone")
						->join("player_screen AS s ON r.screen_id = s.id")
						->join("player_user AS u ON s.uid = u.uid")
						->join("player_player AS p ON r.screen_id = p.id")
						->where($map)
						->select();
			}else if($user_model->is_agent($uid, $role_id, $roles)){
				$uids = $user_model->where("puid={$uid}")->getField("uid", true);
				$map['s.uid'] = array("IN", $uids);
				return $this
						->alias("r")
						->field("r.*,p.name,u.email,u.phone")
						->join("player_screen AS s ON r.screen_id = s.id")
						->join("player_user AS u ON s.uid = u.uid")
						->join("player_player AS p ON r.screen_id = p.id")
						->where($map)
						->select();
			}else if($user_model->is_normal($uid, $role_id, $roles)){
				$map['s.uid'] = $uid;
				return $this
						->alias("r")
						->field("r.*,p.name,u.email,u.phone")
						->join("player_screen AS s ON r.screen_id = s.id")
						->join("player_user AS u ON s.uid = u.uid")
						->join("player_player AS p ON r.screen_id = p.id")
						->where($map)
						->select();
			}else{
				return array();
			}
		}
	}
}
