
import { Season, SeasonInformation, NewSubscription, SeasonCollection } from '../../models';
import { baseApi } from '../../base';
import { animeListStore } from './anime-list.store';
import { userStore } from '..';
import { safeRequestExecute, safeRequestExecuteWithLoader, presentToast } from '../../helpers';

function fetchSeasonCollection(season: Season, year: number) {
  safeRequestExecuteWithLoader(async () => {
    const result = await baseApi.get(`library/${year}/${season}`)
      .json<SeasonCollection>();
    animeListStore.update(state => ({
      ...state,
      ...{
        animes: result.animes,
        seasonInfo: {
          season: result.season,
          year: result.year
        }
      }
    }));
  })
}

function fetchLatestSeason() {
  safeRequestExecuteWithLoader(async () => {
    const result = await baseApi.get(`library/latest`)
      .json<SeasonCollection>();
    animeListStore.update(state => ({
      ...state,
      ...{
        animes: result.animes,
        seasonInfo: 'latest'
      }
    }));
  });
}

function fetchAvailableSeasons() {
  safeRequestExecuteWithLoader(async () => {
    const result = await baseApi.get(`seasons`)
      .json<SeasonInformation[]>();
    animeListStore.update(state => ({
      ...state,
      availableSeasons: result,
    }));
  })
}

function addSubscription(subscription: NewSubscription) {
  safeRequestExecute(async () => {
    await baseApi.post(`subscriptions`, { json: subscription });
    userStore.update(state => {
      return { ...state, subscriptions: [...state.subscriptions, subscription.animeId] }
    })
    presentToast({
      color: 'success',
      duration: 3000,
      message: `${subscription.animeId} has been added to your subscriptions`,
    });
  })
}


export {
  fetchSeasonCollection,
  fetchLatestSeason,
  fetchAvailableSeasons,
  addSubscription
}

// export const animeListService = new AnimeListService(animeListStore);
