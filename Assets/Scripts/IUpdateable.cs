using System;

public interface IUpdateable
{
	bool Tick(DateTime currentTime);
}