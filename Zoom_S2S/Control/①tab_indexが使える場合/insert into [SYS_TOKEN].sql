select * from [SYS_TOKEN];

insert into [SYS_TOKEN] 
	values(
		'T02',
		'ZOOM_S2S_COUNT',
		'0',
		GETDATE(),
		null,
		'SYSTEM',
		null
		),
		
		(
		'T03',
		'ZOOM_S2S_MAXCOUNT',
		'9',
		GETDATE(),
		null,
		'SYSTEM',
		null
		)
		
		;


select * from [SYS_TOKEN];