<?php
return array(
	//'配置项'=>'配置值'
	'MODULE_ALLOW_LIST'	=>	array('Api','Home','Index', 'Admin'),
	'MODULE_DENY_LIST'	=>	array('Common','Runtime'),
	'DEFAULT_MODULE'	=>	'Index',
	//数据库配置
	'DB_TYPE'		=>  'mysql',		// 数据库类型
	'DB_HOST'		=>  '127.0.0.1',	// 服务器地址
	'DB_NAME'		=>  'player',		// 数据库名
	'DB_USER'		=>  'root',			// 用户名
	'DB_PWD'		=>  '',				// 密码
	//'DB_USER'		=>	'root',      	// 用户名
	//'DB_PWD'		=>	'lkgx',			// 密码
	'DB_PORT'		=>  '3306',			// 端口
	'DB_PREFIX'		=>  'player_',		// 数据库表前缀
    
    'pwd_auth_key'	=>	'yFjXp2Qxke',	//用户密码加密密钥
);