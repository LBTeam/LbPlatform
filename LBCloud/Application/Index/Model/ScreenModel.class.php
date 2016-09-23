<?php
/**
 * 屏幕model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class ScreenModel extends Model
{
	public function screen_by_uid($user_id){
		$map = array();
		$map['s.is_delete'] = 0;
		if(!is_administrator()){
			$cfg_model = D("Config");
			$user_model = D("User");
			$roles = $cfg_model->roles();
			if($user_model->is_agent($user_id, 0, $roles)){
				$map['u.puid'] = $user_id;
			}elseif($user_model->is_normal($user_id, 0, $roles)){
				$map['s.uid'] = $user_id;
			}
		}
		return $this
				->alias("s")
				->field("s.*,g.id AS group_id,g.name AS group_name,u.email AS u_email,u.phone AS u_phone")
				->join("player_group_screen AS rela ON rela.screen_id = s.id AND rela.uid = {$user_id}", 'LEFT')
				->join("player_group AS g ON rela.group_id = g.id", 'LEFT')
				->join("player_user AS u ON u.uid = s.uid")
				->where($map)
				->select();
	}
}
