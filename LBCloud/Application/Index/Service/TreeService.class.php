<?php
namespace Index\Service;

class TreeService
{
	/*
	private $tree_list = array();
	
	public function create($list, $pid=0){
		foreach($list as $key => $val){
			if($val['pid'] == $pid){
				$this->tree_list[] = $val;
				unset($list[$key]);
				self::create($list, $val['id']);
			}
		}
		return $this->tree_list;
	}
	*/
	
	public function create_menu($menu, $pid=0){
		$menus = array();
		foreach($menu as $val){
			if($val['pid'] == $pid){
				$children = $this->create_menu($menu, $val['id']);
				if($children){
					$val['children'] = $children;
				}
				$menus[] = $val;
			}
		}
		return $menus;
	}
	
	/*
	public function menu_children($menu, $id=0){
		$childrens = array();
		$childrens[] = $id;
		foreach($menu as $val){
			if($id == $val['pid']){
				$childs = $this->menu_children($menu, $val['id']);
				$childrens = array_merge($childrens, $childs);
			}
		}
		return $childrens;
	}
	 */
	
	/*
	public function childrens($menu, $access){
		$childrens = array();
		foreach($access as $val){
			$childrens[] = $val;
			$childs = array();
			foreach($menu as $v){
				if($val == $v['pid'] && $v['rank'] != 2){
					$childs[] = $v['id'];
				}
			}
			if($childs){
				$temp = $this->childrens($menu, $childs);
				$childrens = array_merge($childrens, $temp);
			}
		}
		return $childrens;
	}
	 */
}
