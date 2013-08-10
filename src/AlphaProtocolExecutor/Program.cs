using lib.AlphaProtocol;
using System.Linq;

namespace AlphaProtocolExecutor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var cases = data
                .Split('\n').Select(l => l.Split(' ').Select(e => e.Trim()).Where(e => e.Length > 0).ToArray())
                .Where(l => l.Length > 0)
                .ToArray();

            foreach (var anCase in cases)
            {
                AlphaProtocol.PostSolution(anCase.First(), 5, anCase.Skip(1).ToArray());
            }
        }


        private static string data =
            @"
Q8CaRCOpwfImQx4ZhFTW3b82              or          shr4                                                                                                                                                                  
mfyNMhQzdmCMfNg62Tgmy9I6            shr1      shr16     shr4                                                                                                                                                   
4femUNIJShAb9006dZ0d1HLH  shl1       shr1       shr16                                                                                                                                                 
0jGUeyP0MGYqOArueOJTKtYj not        or                                                                                                                                                                       
yLKcjwEr4yKa8veMR7xQiOvi     and       not                                                                                                                                                                     
bcoywJtur1SLkDdN01EAXotq    plus      shr1                                                                                                                                                                   
7D4MM2JSk4z3PAqnJoPTQ2e2                plus      shr1                                                                                                                                                                  
XYRkFqpwFHQosZaTpBcf6G8A shr16    shr4                                                                                                                                                                   
XO44ZQ60UB5ANdvvIKcPDudB                and       shr4                                                                                                                                                                  
ZBZ4QmskyN5OxvLbIAE8yxwk plus      shl1                                                                                                                                                                    
QZIBlgDlZpF1IhN6TAcfhdAq      shr1      shr16                                                                                                                                                                 
eCpm2BODNB4mZmdAL4yk5gcs             shl1       shr4                                                                                                                                                                  
Flqq8uR7sQTFvSnicuZ1xExB       shl1       shr4                                                                                                                                                                   
NyPEtWJUrjX8ZOHEPAVnUU5E               or          shr4                                                                                                                                                                  
MbAyJYNmtAhFjzLXhvAP2aFo shr4      xor                                                                                                                                                                     
60jQHIgfQ58BrK6SSzxBX0tF       and       shr1                                                                                                                                                                   
WDhkXOhAhnPhHiDBarJonAio shl1       shr1       shr16                                                                                                                                                 
8t9femWkXdGVbBCG3a4zJAXM             and       shr1                                                                                                                                                                  
d9j2N4RMRAzB61yVpHnbDP0q               shl1       xor                                                                                                                                                                    
E0oInyFXTpkmruhUl8TmEpBW and       not                                                                                                                                                                     
WwQEDeMTeyzJnNACAKrBJTrL              and       not                                                                                                                                                                    
N0tHhhLefKUz7yAsRohBAWnV               or          shr16                                                                                                                                                                
VV6hBrhGG9fLduidk5lb6RAS    not        plus                                                                                                                                                                   
eGrtKhBkpM6J03QJewm8Xw3J               plus      shr16                                                                                                                                                                
tkdS0AVJjSEIAGIt6xVTGilZ         plus      shr4                                                                                                                                                                   
YoXpKHJ1Efhxx1l0TngcAtyU      not        plus                                                                                                                                                                   
CQ8jyFd6PxZQmGZmywgET15U              shr1      xor                                                                                                                                                                    
ux1SP8EQHfAohYKS9IAwkuJ6   plus      shr1                                                                                                                                                                   
RFSXYz3oviacWdfaQ43FaAwE   not        shr1                                                                                                                                                                   
yiJi4Z7tULYiomEnFHrCInHz         not        xor                                                                                                                                                                     
uXPybAw4fX0HeZ8JUD9JJu9Q  and       shr1                                                                                                                                                                   
Iqi58s11prC5nIzbU4ZdrpzN        plus      shr4                                                                                                                                                                   
KDkxuUBYuePRGcaEAzZ190Rg shl1       shr1                                                                                                                                                                   
Ss4QrA2i4YA6OAk7RjceweZp    or          shr4                                                                                                                                                                   
4SIhfYKMkK7BGjhBBzp1FiEi       shr4      xor                                                                                                                                                                     
SkO06vvTMtDorn11x8TDpbuB  plus      shl1                                                                                                                                                                    
oFYyl2wysXuSH33dAJOMmASB               not        shr1       shr16                                                                                                                                                
YNn5vMxP0dPZfTSC9eDqqxkV                plus      shr16                                                                                                                                                                
jJ1gSEsfE1v9JPLs3NBwv3xU       shr16    xor                                                                                                                                                                     
zrfUYBAHgYbroGaM62EeAtob  not        or                                                                                                                                                                       
jaAL600BTvbRHldHoWgJ8Ang   shl1       shr1                                                                                                                                                                   
Opb1LYH3K897Mmzx1KwCEFyP              shl1       shr16                                                                                                                                                                
WqO3t453G1uxu6z6xsWzmu8t                shr4      xor                                                                                                                                                                    
fY7SnLZq6d0AknZvfL0TUB2W   or          shr1                                                                                                                                                                   
jcbn0ihk8W3Z3BkJHphKywtI     not        shr1       shr16                                                                                                                                                 
RtWDQVKQDdCel6zWUbXNH3OZ           or          shl1                                                                                                                                                                   
19Q83DMXdqAEXPrnYdge7UUF              or          shl1                                                                                                                                                                   
Ze0wKKGydfAgAYHXYSKS6rWM             not        plus                                                                                                                                                                   
04ObKLFwRdb0rSSBqwS31LeP or          shr1                                                                                                                                                                   
gfrPXZxEFThRdYoYyw2mO0A2 not        shr4                                                                                                                                                                   
P45vt0ONVApt0FcUhIs1hBrk    not        shr1       shr16                                                                                                                                                 
ZN6ARvgWN9d71QUOK6ZNsxvP             or          shr1                                                                                                                                                                  
QyFFWzXiN8gA2by6T3lFxATp   and       shl1                                                                                                                                                                   
QcaLcRaeHby2oCW9WiFHk0sJ  not        or                                                                                                                                                                       
WvMQW7j4dYac2QD6AEFknLZB              shl1       shr4                                                                                                                                                                  
QHHB5yHcGIQIJK2JqsbkBRI9     plus      shr4                                                                                                                                                                   
BLk1tYAzsX7AfgACizR0LhLl         or          shl1                                                                                                                                                                    
PXZADHM7HDliwfKo1WAb0o5n              or          shr16                                                                                                                                                                
HqLXLHDz8NobbcYsH622oZM5 and       not                                                                                                                                                                     
LUgAnWrcgcvNQA2JwCUOnEVF             shr1      shr16                                                                                                                                                                
q3gdvVkcbdJoTJeRudYy505f     and       shr4                                                                                                                                                                   
mBEwPpLABNn5J36mE1itLTcQ or          shr1                                                                                                                                                                   
BM2c5xUPewLhdOXAMNeZPQAE          not        plus                                                                                                                                                                   
J91LqzLMEIf10CfoMtF4uYFm    or          shr16                                                                                                                                                                 
IU3A6yfvX9aMfKKo6PqH20yA  not        shr16                                                                                                                                                                 
RnaUkaa6u7yunR9ReD74uMWn             shl1       xor                                                                                                                                                                    
XZYGIbIdYqGWTFsVKZrbxhpS  not        or                                                                                                                                                                       
dsFSS2HwvEaAwEi4Dh9knDVC not        shr1       shr4                                                                                                                                                   
vtr5atJNtzI5m7j5G3dnVNJA      shr4                                                                                                                                                                                  
KIyu4dOjxBw5MQX0XBRY7GuD              plus      shr4                                                                                                                                                                  
Z34zfwbhynJJPQgb2fOxV9vG   not        xor                                                                                                                                                                     
wQUTX0DCGN4JC0bW2T2SnbXI              shr4      xor                                                                                                                                                                    
T1jQtM9FzhKFkfePBKbarDiy     not        shr4                                                                                                                                                                   
I5Gh0h74c3MLxhLpibUAxFMG not        plus                                                                                                                                                                   
JUC6kKtzWTIfAbkbYwAeeShB  and       shr1                                                                                                                                                                   
EKgMABMJf6HLPCC7liTbVQ1S  not        plus                                                                                                                                                                   
ACFmL4Nj3SIBmzbk62ZTbyVs   and       not                                                                                                                                                                     
iOXfTecP2d2Ndm9PiYH4k0wS   shr16    xor                                                                                                                                                                     
KyAuY8lfAi9Ay17fuowOsG6P    not        plus                                                                                                                                                                   
54nsJp5CFJOBj1K7gLU3Mb6D   not        plus                                                                                                                                                                   
8HLA1WgJpppfVxagINLCaMPi  not        plus                                                                                                                                                                   
DGTd9jUlIt709yyiRwcFySZG       and       shr16                                                                                                                                                                 
cqheruC3Bo78EMlMyRQktLBP  shl1       xor                                                                                                                                                                     
nMRyS8GXo0ARJUbSLV9yAzMb             plus      shl1                                                                                                                                                                   
P8vMmGR8Hv7iAIBxPBsvc0Nn and       not                                                                                                                                                                     
f3KWpRWcPDG4D0Ei8bMztG4F               shr1      shr4                                                                                                                                                                  
YNkFFNAxo5ZwkppZuulAnoMu              shl1       shr1       shr16                                                                                                                                                
b4Opa1X6VYB0gIYu21NoeF8w plus      shl1                                                                                                                                                                    
P1VgBfTkeVqYn8yNbHgphPjJ   shl1       shr16                        
";
    }
}