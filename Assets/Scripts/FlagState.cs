using System;

public class FlagState
{
	public event Action<FlagState> FlagStateChanged;
	private Flag flag;

	public Flag Flag
	{
		get => flag;
		set
		{
			if (value != flag)
			{
				flag = value;
				FlagStateChanged?.Invoke(this);
			}
		}
	}

	private FlagArea area;

	public FlagArea Area
	{
		get => area;
		set
		{
			if (value != area)
			{
				area = value;
				FlagStateChanged?.Invoke(this);
			}
		}
	}

	public FlagState(Flag flag, FlagArea area)
	{
		this.flag = flag;
		this.area = area;
	}

	public override string ToString() => $"{Enum.GetName(typeof(Flag), flag)}{Enum.GetName(typeof(FlagArea), area)}";
}