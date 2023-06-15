select * from [SYS_TOKEN];

insert into [SYS_TOKEN] 

	values(
		'T02',
		'ZOOM_S2S_Token',
		'',
		'2023-06-13',
		null,
		'SYSTEM',
		null
		),
		
		(
		'T03',
		'ZOOM_S2S_Interval',
		'1800',
		GETDATE(),
		null,
		'SYSTEM',
		null
		)
		
		;

select * from [SYS_TOKEN];