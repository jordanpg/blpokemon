function searchWords(%sourceString, %searchString)
{
	if(%searchString $= "")
		return -1;

	%ct = getWordCount(%sourceString);
	for(%i = 0; %i < %ct; %i++)
	{
		if(getWord(%sourceString, %i) $= %searchString)
			return %i;
	}
	return -1;
}

function bubbleSort(%list)
{
	%ct = getFieldCount(%list);
	for(%i = 0; %i < %ct; %i++)
	{
		for(%k = 0; %k < (%ct - %i - 1); %k++)
		{
			if(firstWord(getField(%list, %k)) > firstWord(getField(%list, %k+1)))
			{
				%tmp = getField(%list, %k);
				%list = setField(%list, %k, getField(%list, %k+1));
				%list = setField(%list, %k+1, %tmp);
			}
		}
	}
	return %list;
}