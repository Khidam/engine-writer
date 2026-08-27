# Engine Writer

**Engine Writer** é um protótipo de aplicativo/engine de escrita em **Unity + C#** para autores de webnovel. A proposta é transformar a sessão de escrita em uma pequena operação estratégica: escrever alimenta um mapa em relevo, o rádio entrega missões, e dois agentes locais — **Builder** e **Critic** — mantêm um ciclo leve de melhoria sem tirar o autor do fluxo.

## Conceito

- Tema visual original inspirado em pôsteres comerciais e ilustração editorial do século XX, com clima de 1944, espionagem e comunicação por rádio.
- Mapas alternáveis entre visão **2D tática** e **3D em relevo low-poly**.
- Simulação de quatro frentes/lane-based, inspirada na sensação de jogos de tabuleiro/cartas por terreno, sem copiar personagens, assets ou regras proprietárias.
- Editor de capítulo com autosave, contagem de palavras (e caracteres Han no perfil CN), análise de gancho/ritmo/diálogo/cliffhanger e exportação TXT/Markdown.
- Perfis de ritmo para webnovels **CN / KR / EN**.
- Loop de motivação: palavras -> Signal -> turnos do mapa -> rádio -> micro-missão -> recompensa de sessão.

## Abrir no Unity

1. Instale **Unity 6.x** (o projeto foi preparado para a linha 6000.0).
2. Abra esta pasta no Unity Hub como projeto existente.
3. Aguarde a compilação.
4. No menu do Unity, use **Engine Writer > Create / Refresh Demo Scene**.
5. Abra `Assets/EngineWriter/Scenes/EngineWriter.unity` e pressione **Play**.

O bootstrap também tenta iniciar o app automaticamente em qualquer cena vazia durante Play Mode.

## Atalhos

- `Ctrl+S`: salvar sessão.
- `F6`: executar o ciclo Critic -> Builder.
- `F7`: alternar mapa 2D / 3D.

## Estrutura

- `Assets/EngineWriter/Scripts/Core` — sessão, bootstrap e loop principal.
- `Assets/EngineWriter/Scripts/World` — mapa em relevo, frentes e simulação.
- `Assets/EngineWriter/Scripts/Agents` — CriticAgent + BuilderAgent.
- `Assets/EngineWriter/Scripts/Webnovel` — análise e perfis CN/KR/EN.
- `Assets/EngineWriter/Scripts/Persistence` — save/export JSON/TXT/MD.
- `Assets/EngineWriter/Scripts/UI` — mesa de escrita, rádio, painéis e controles.
- `Assets/EngineWriter/Editor` — criação automática da cena de demonstração.

## Filosofia do produto

O escritor não deve sentir que está “preenchendo métricas”. O mapa e o rádio existem como feedback ambiental: pequenas consequências visuais, missões curtas e sinais de progresso. A crítica é opcional e condensada a poucas observações práticas.

## Licença

MIT para o código deste repositório. Não inclui nem depende de assets de Team Fortress 2, Adventure Time/Card Wars ou obras de J.C. Leyendecker, Dean Cornwell e Norman Rockwell. As referências são apenas de direção de clima, composição e época; use arte original ou licenciada no produto final.
