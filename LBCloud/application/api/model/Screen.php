<?php
/**
 * 屏model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace app\api\model;

use think\Model;

class Screen extends Model
{	
	public function user_all_screen($user_id){
		return $this
				->alias("s")
				->field("s.*,g.name AS group_name")
				->join(array("group_screen"=>"rela"), "rela.screen_id = s.id", 'LEFT')
				->join(array("group"=>"g"), "g.id = rela.group_id", 'LEFT')
				->where("s.uid = {$user_id}")
				->select();
	}
}
