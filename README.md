# Prince Titan

**Prince Titan** é uma engine de escrita em Unity com um mundo vivo sempre visível ao lado do manuscrito. O objetivo é abrir o aplicativo, escrever e, quando quiser respirar, simplesmente observar aviões, robôs, mercados, companhias, casas e relações de poder se movendo no atlas.

## O que já funciona

- editor de capítulos com título, contagem de palavras e atalhos;
- autosave local em JSON e backup automático;
- exportação do capítulo ativo em UTF-8 `.txt`;
- interface Unity redesenhada como uma mesa de operações de espionagem, com materiais escuros, marfim, latão e magenta;
- duas placas de arte HD originais: mesa de escrita secreta e atlas cartográfico vivo em pergaminho;
- mapa lateral e atlas expandido com arte cartográfica HD e uma camada procedural de inteligência em tempo real;
- 14 lugares visíveis: cidades, mercados, empresas, casas, porto, aeródromo, relé e fábricas de robôs;
- aviões em rotas contínuas e um transporte robótico em movimento;
- atividade dos mercados, relógio mundial e eventos ambientes;
- quatro poderes que disputam influência: Império, Governo, Clã e Empreiteira;
- filtros por poder sem esconder permanentemente nenhum dado;
- árvore biológica e política com nome, família, origem, nascimento, função e aliança de cada pessoa;
- simulação independente da escrita: escrever nunca altera artificialmente quem vence o mapa.
- entrada moderna e redundante para mouse/teclado, com foco automático, estados de hover/pressionado e indicador `INPUT ONLINE`;

## Direção visual

A identidade é original: espionagem editorial de meados do século, cartões de inteligência, carvão, branco marfim, magenta e latão. O mapa mistura papel aquecido pelo sol, tinta gravada, cidades, ferrovias, portos, indústria, rotas pontilhadas e movimento discreto. As imagens de referência serviram apenas para definir clima e composição; nenhum asset, personagem, arma, logotipo ou interface proprietária foi copiado.

## Baixar e abrir rapidamente

1. Abra **Releases** neste repositório.
2. Baixe `PrinceTitan-Windows-x64.zip` da release mais recente.
3. Extraia o ZIP.
4. Dê dois cliques em `PrinceTitan.exe`.

Você não precisa abrir o Unity nem compilar no seu PC para usar uma release pronta.

## Primeiro build no GitHub

O GitHub Actions precisa ativar a sua licença Unity Personal uma única vez. Adicione estes três Secrets em **Settings → Secrets and variables → Actions**:

- `UNITY_LICENSE`: conteúdo completo de `C:\ProgramData\Unity\Unity_lic.ulf`;
- `UNITY_EMAIL`: e-mail da conta Unity;
- `UNITY_PASSWORD`: senha da conta Unity.

Depois abra **Actions → Prince Titan Unity Windows Release → Run workflow**. O workflow compila no servidor e cria a release com o ZIP automaticamente. Nunca coloque senha ou licença em arquivo do repositório, issue ou conversa.

## Dados locais

No Windows, o projeto e os exports ficam dentro da pasta persistente do Unity, em `AppData/LocalLow/Khidam/Prince Titan/PrinceTitan`. O arquivo principal é `project.json`; cada gravação também mantém `project.json.backup`.

## Código anterior preservado

Os protótipos HTML/WinForms anteriores continuam nas pastas `app/`, `launcher/` e `src/`. A implementação canônica nova é o projeto Unity em `Assets/`, `Packages/` e `ProjectSettings/`.

Licença MIT para o código original deste repositório.
