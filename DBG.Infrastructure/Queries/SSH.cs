namespace DBG.Infrastructure.Queries;

public class SSH
{
    public static string GetCpuUsage()
    {
        return
            "IFS=',' read -ra a <<< $(top -bn1 | grep '%Cpu(s):');b=$(echo \"${a[3]}\" | awk '{print $1}');b=$(echo \"100 - $b\" | bc -l);echo $b";
    }

    public static string GetRamUsage()
    {
        return "free -m | awk '/Mem/ {print $3}'";
    }

    public static string GetDiskUsage()
    {
        return
            "echo \"{\" && df -B 1 | awk 'NR>1 && $1 ~ /^\\/dev\\// {printf \"  \\\"%s\\\": {\\\"used\\\": \\\"%s\\\", \\\"capacity\\\": \\\"%s\\\", \\\"available\\\": \\\"%s\\\", \\\"percentage\\\": \\\"%s\\\", \\\"mounted\\\": \\\"%s\\\"},\\n\", $1, $3, $2, $4, $5, $6}' | sed '$s/,$//' && echo \"}\"";
    }

    public static string GetVersion()
    {
        return "cat /etc/os-release | grep PRETTY_NAME | awk -F '=' '{print $2}' | tr -d '\"'";
    }

    public static string GetCpuCount()
    {
        return "cat /proc/cpuinfo | grep processor | wc -l";
    }

    public static string GetRamCount()
    {
        return "cat /proc/meminfo | grep MemTotal | awk '{print $2}'";
    }

    public static string GetUptime()
    {
        return "a=$(uptime -p);b=\"${a#up }\";echo $b";
    }
}