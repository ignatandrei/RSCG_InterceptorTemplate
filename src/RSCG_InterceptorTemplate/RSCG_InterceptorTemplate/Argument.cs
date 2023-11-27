

namespace RSCG_InterceptorTemplate;

public partial struct Argument
{
    public Argument(string typeAndName)
    {
        this.TypeAndName = typeAndName;
        this.Type=typeAndName.Split(' ')[0];
        this.Name=typeAndName.Split(' ')[1];
    }
    public string TypeAndName { get; }
    public string Type { get; }
    public string Name { get; }
}
