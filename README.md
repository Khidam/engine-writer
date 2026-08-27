# Engine Writer — World Desk

Engine Writer é uma engine de escrita para webnovels que combina uma **mesa de escrita calma** com um **mundo militar vivo em 1944**. A regra de produto é simples: quando você quer escrever, o texto domina; quando quer brincar com o cenário, o mapa vira a mesa principal.

## World Desk preview

A versão atual roda em um HTML offline leve e possui um launcher Windows pequeno. Não exige Unity e não embute um runtime .NET de centenas de megabytes.

### WRITE DESK

- trilho de capítulos e autosave local;
- perfis EN / CN / KR;
- editor em papel amplo, sem dashboard agressivo;
- `Testar capítulo`: no máximo três achados com evidência textual e sem reescrever a prosa;
- `Ler`: modo leitor limpo para sentir o capítulo como leitor;
- `Publicar`: exporta capítulo UTF-8 e manifesto local;
- `Quiet Write`: reduz o mapa a uma presença visual discreta;
- rádio ambiente procedural opcional.

### WORLD TABLE

- mapa alternável entre tático 2D e relevo em perspectiva;
- terreno com água, planície, floresta, terreno seco e elevação rochosa;
- cidades, estradas e linha de frente calculada pela influência real das forças;
- infantaria, blindados, artilharia e unidades de suprimento;
- HP, moral, fadiga, munição, combustível e suprimento;
- movimento afetado por terreno e clima;
- combate, retirada, captura de cidades e destroços persistentes;
- aviação com base, combustível, munição, missões CAP/recon/strike, retorno e combate aéreo;
- clima dinâmico com visibilidade e penalidades de movimento/aviação;
- produção de suprimento nas cidades e consumo logístico das unidades;
- rádio de campo registrando eventos realmente produzidos pela simulação.

Escrever não concede bônus artificiais às forças. A escrita só libera resumos discretos do que aconteceu no mundo; o resultado militar vem da simulação.

## Download

Abra a aba **Releases** e procure a release mais recente `Engine Writer World Desk`. O ZIP contém:

- `EngineWriter.exe` — launcher pequeno;
- `EngineWriter.html` — aplicativo/simulação offline;
- `README.md`.

O launcher usa o Microsoft Edge instalado no Windows em modo aplicativo e, se não encontrar Edge, abre o HTML no navegador padrão.

## Direção artística

A interface usa uma direção original inspirada por comunicação militar de 1944, ilustração comercial/editorial do século XX, formas gráficas fortes e contraste de equipes. Não inclui assets, personagens ou arte proprietária de Team Fortress 2, Adventure Time/Card Wars, J.C. Leyendecker, Dean Cornwell ou Norman Rockwell.

## Próximas prioridades

1. elevar o relevo para WebGL/Three.js offline mantendo o pacote pequeno;
2. logística por estrada/depósito em vez de distância simples;
3. ordens operacionais e rotas editáveis;
4. inteligência incompleta/fog of war;
5. dano de veículos e aeronaves por subsistema;
6. conectar lugares e acontecimentos do mapa ao contexto do capítulo sem transformar o editor em uma Story Bible.

## Licença

MIT para o código original deste repositório.
