<?php
return array(
	//'配置项'=>'配置值'
	'oss_part_size'			=>	10485760,	//oss上传分片大小
	'oss_100K_size'			=>	102400,		//100K大小
	'oss_200K_size'			=>	204800,		//100K大小
	'oss_1M_part_size'		=>	1048576,	//1M大小
	'oss_2M_part_size'		=>	2097152,	//2M大小
	'oss_5M_part_size'		=>	5242880,	//5M大小
	'oss_10M_part_size'		=>	10485760,	//10M大小
	'oss_50M_part_size'		=>	52428800,	//50M大小
	
	'player_program_subfix'	=>	'playprog',	//播放方案文件后缀名
	
	//未登录可访问模块
	'manager_not_logged' => array(
		'login',
		'refresh_token',
	),
	//空数据可访问模块
	'manager_param_empty' => array(
		'refresh_token',
		'configuration',
		'screens',
		'programs',
		'heartbeat'
	),
);