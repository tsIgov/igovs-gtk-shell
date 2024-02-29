namespace Igs.TypedIds;

public abstract record TypedId(string Value)
{
	public sealed override string ToString() => Value.ToString();
}
