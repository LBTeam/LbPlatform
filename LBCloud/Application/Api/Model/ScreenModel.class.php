<?php
/**
 * 屏model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Model;
use Think\Model;

class ScreenModel extends Model
{
	public function user_all_screen($user_id){
		return $this
				->alias("s")
				->field("s.*,g.name AS group_name")
				->join("player_group_screen AS rela ON rela.screen_id = s.id")
				->join("player_group AS g ON rela.group_id = g.id")
				->where("s.uid = {$user_id}")
				->select();
	}
}
