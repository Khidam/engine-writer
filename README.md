# Prince Titan

**Prince Titan** é uma engine nativa de mundo e escrita construída em Unity. Não é uma página HTML nem um editor de texto coberto por uma imagem: o aplicativo é dividido em salas cinematográficas que representam partes diferentes do universo do autor.

## O que existe na reconstrução 0.3

- **Sala de Comando:** entrada cinematográfica com cinco grandes portas e o pulso geral do mundo.
- **Mapa Vivo:** mapa amplo com zoom e arraste, 14 lugares iniciais, aeronaves e Titãs em movimento, rotas, filtros e influência em tempo real.
- **Escrita:** manuscrito operado como uma máquina física, capítulos, contagem de palavras, autosave e exportação.
- **Pessoas:** árvore biológica e política clicável, famílias, origem, função, ano de nascimento e aliança.
- **Poderes:** Império, Governo, Clã e Empreiteira controlam regiões com influência variável; cada região tem uma placa visual exclusiva.
- **Economia:** mercados, companhias, casas, cidades, aeródromos, portos, relés e fábricas de robôs.
- **Criação de mundo:** novas pessoas e novos lugares podem ser registrados dentro do aplicativo e aparecem imediatamente na simulação.
- **Conforto:** fonte grande por padrão, interface escalável em 100%, 125%, 150% ou 175%, janela redimensionável e tela cheia opcional.
- **Atmosfera:** catorze cenas QHD originais, quatro observatórios de local e um ambiente sonoro mecânico suave que pode ser desligado.

O magenta e o branco são usados como sinais sobre carvão, marfim, madeira escura e latão. As referências fornecidas serviram somente para direção de clima: não há personagem, objeto, símbolo, interface ou mapa copiado.

## Baixar e abrir

1. Abra **Releases** neste repositório.
2. Baixe **PrinceTitan-Windows-x64.zip** da versão mais recente.
3. Extraia todo o ZIP.
4. Dê dois cliques em **PrinceTitan.exe**.

O usuário final não precisa abrir o Unity e não precisa compilar.

## Controles

- clique nos nomes das salas para navegar;
- no Mapa Vivo, arraste para mover e use a roda do mouse para aproximar;
- Ctrl+S salva, Ctrl+E exporta o capítulo e Ctrl+N cria um capítulo;
- Esc fecha uma janela ou retorna à Sala de Comando.

## Dados locais

O projeto e os exports ficam em **AppData/LocalLow/Khidam/Prince Titan/PrinceTitan**. O arquivo principal é **project.json**; cada salvamento mantém **project.json.backup**. Projetos antigos são migrados sem apagar os capítulos já escritos.

## Build automático

O GitHub Actions usa Unity 2022.3 para montar a versão Windows x64 e publicar uma Release. A pipeline valida o executável, os dados QHD e o tamanho extraído antes de publicar.

Licença MIT para o código original.
