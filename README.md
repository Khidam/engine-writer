# Prince Titan

**Prince Titan** é uma engine nativa de escrita e simulação de mundo feita em Unity para Windows. A versão 0.4 abandona o antigo painel plano: o aplicativo agora funciona como um posto clandestino de 1944, com mesa de relevo 3D, operações em movimento e arquivos mecânicos ligados à história.

## O que existe na versão 0.4

- **Bunker 1944:** rádio de válvulas, cifras, reconhecimento aéreo, plantas roubadas e interceptações que vêm do estado real da simulação.
- **Simulação de relevo 3D:** terreno esculpido e iluminado, câmera orbital, zoom, dois mundos selecionáveis, edifícios, hangares, casas de época, robôs e aeronaves com hélice em movimento.
- **Operações rastreáveis:** cada unidade tem indicativo, origem, destino, rota visível, objetivo, carga, contexto, horário de partida, estado e ETA. A unidade percorre a rota uma única vez e gera uma ocorrência ao chegar.
- **Mundo Real e Dimensão Quebrada:** a camada quebrada tem relevo, floresta reclamada, fraturas e uma travessia que pode durar dias, meses ou anos.
- **Dossiê de escrita:** fonte de máquina de escrever, capítulos, contexto de local/personagem/máquina, telegramas da simulação, contagem de palavras, autosave e exportação.
- **Exclusão segura:** capítulos pedem confirmação; projetos exigem digitar `APAGAR` e são movidos para uma pasta recuperável antes de um novo arquivo ser criado.
- **Organização e Torre de Troia:** Príncipe, Sussurro Fantasma, Titã e toda a formação têm função, habilidade, origem, ligação e posição próprias.
- **Arquivo de guerra:** máquinas, organizações, locais e gravações trocam a chapa de observação conforme o registro. Danos de cabeça e resfriamento persistem e aparecem no robô 3D.
- **Escala coerente:** robô de arena de 2,3 m, robô de carga, robô lutador gigante com comando abdominal, Titã humanoide e três aeronaves fictícias inspiradas na engenharia de 1944, sem insígnias históricas.
- **Conforto:** tipografia interativa mínima de 17 pontos, escala de 100%, 125%, 150% ou 175%, janela redimensionável e tela cheia opcional.

Não existe uma categoria de “Poderes” nem barras de influência. Império, Governo, Clã e Empreiteira são tipos de organização; habilidades pertencem às pessoas e às técnicas narrativas.

## Baixar e abrir

1. Abra **Releases** neste repositório.
2. Baixe **PrinceTitan-Windows-x64.zip** da versão mais recente.
3. Extraia o ZIP inteiro.
4. Abra **PrinceTitan.exe**.

O usuário final não precisa abrir o Unity e não precisa compilar.

## Controles

- clique nas abas superiores para trocar de sala;
- na Simulação 3D, botão esquerdo + arraste orbita, botão direito + arraste desloca e a roda aproxima;
- clique em uma miniatura ou cartão para abrir seu dossiê e use **SEGUIR SINAL** para acompanhar a unidade;
- alterne entre **MUNDO REAL** e **DIMENSÃO QUEBRADA**;
- Ctrl+S salva, Ctrl+E exporta, Ctrl+N cria e Ctrl+D duplica o capítulo;
- Esc fecha uma janela ou retorna ao Bunker.

## Dados locais

Projetos e exports ficam em **AppData/LocalLow/Khidam/Prince Titan/PrinceTitan**. O arquivo principal é **project.json** e o backup é **project.json.backup**. Um projeto apagado é preservado em **Deleted Projects** e pode ser restaurado pelo aplicativo. Projetos das versões anteriores mantêm os capítulos escritos e recebem o novo mundo simulado.

## Build automático

O GitHub Actions usa Unity 2022.3.62f1 para montar Windows x64, validar `PrinceTitan.exe`, conferir os dados do jogador e exigir pelo menos 200 MiB extraídos de conteúdo funcional antes de publicar a Release. As onze chapas QHD são usadas nas salas e nos registros; não há arquivo de preenchimento.

Licença MIT para o código original. Fontes DejaVu sob a licença Bitstream Vera; consulte [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
