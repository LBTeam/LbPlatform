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
		return $this
				->alias("s")
				->field("s.*,g.id AS group_id,g.name AS group_name")
				->join("player_group_screen AS rela ON rela.screen_id = s.id", 'LEFT')
				->join("player_group AS g ON rela.group_id = g.id", 'LEFT')
				->where("s.uid = {$user_id} AND s.is_delete = 0")
				->select();
	}
}
